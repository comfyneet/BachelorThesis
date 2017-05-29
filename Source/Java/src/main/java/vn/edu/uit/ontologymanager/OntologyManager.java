package vn.edu.uit.ontologymanager;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import org.jetbrains.annotations.NotNull;
import org.jetbrains.annotations.Nullable;
import org.semanticweb.HermiT.ReasonerFactory;
import org.semanticweb.owlapi.apibinding.OWLManager;
import org.semanticweb.owlapi.model.*;
import org.semanticweb.owlapi.reasoner.*;
import org.semanticweb.owlapi.search.Searcher;
import org.semanticweb.owlapi.vocab.OWLRDFVocabulary;
import vn.edu.uit.ontologymanager.model.*;
import vn.edu.uit.ontologymanager.model.Class;
import vn.edu.uit.server.GetType;
import vn.edu.uit.server.Request;
import vn.edu.uit.server.RequestType;
import vn.edu.uit.server.Response;
import vn.edu.uit.shared.Pair;
import vn.edu.uit.shared.StringUtils;

import java.io.File;
import java.util.*;
import java.util.logging.Level;
import java.util.logging.Logger;

import static vn.edu.uit.ontologymanager.model.DataType.*;
import static vn.edu.uit.server.GetType.GET_ALL;
import static vn.edu.uit.server.GetType.GET_DIRECT;
import static vn.edu.uit.server.ResponseType.*;

public class OntologyManager {

    @NotNull
    private static final Logger LOGGER = Logger.getLogger(OntologyManager.class.getName());
    @NotNull
    private static final String DEFAULT_LANG = "vi";
    @NotNull
    private final Gson gson;
    @NotNull
    private final OWLDataFactory factory;

    @NotNull
    private final OWLOntology ontology;

    @NotNull
    private final OWLReasoner reasoner;

    @NotNull
    private final String prefix;

    public OntologyManager(@NotNull final File file) throws OWLOntologyCreationException {
        gson = new GsonBuilder().create();

        final OWLOntologyManager manager = OWLManager.createOWLOntologyManager();

        factory = manager.getOWLDataFactory();
        ontology = manager.loadOntologyFromOntologyDocument(file);

        prefix = ontology.getOntologyID().getOntologyIRI().get() + "#";

        final OWLReasonerFactory reasonerFactory = new ReasonerFactory();
        final ConsoleProgressMonitor progressMonitor = new ConsoleProgressMonitor();
        final OWLReasonerConfiguration config = new SimpleConfiguration(progressMonitor);
        reasoner = reasonerFactory.createReasoner(ontology, config);

        reasoner.precomputeInferences();
        if (!reasoner.isConsistent()) LOGGER.warning("Ontology is inconsistent.");

        LOGGER.info("Ontology is loaded.");
    }

    @NotNull
    public final String process(@NotNull final String message) {
        final Request request = gson.fromJson(message, Request.class);
        final Map<String, Object> data = request.getData();

        Response response;
        try {
            final RequestType type = request.getType();
            switch (type) {
                case SEARCH_INDIVIDUALS:
                    response = parseSearchIndividuals(data);
                    break;
                case GET_CLASS:
                    response = parseClass(data);
                    break;
                case GET_SUPERCLASSES:
                    response = parseSuperClasses(data);
                    break;
                case GET_SUBCLASSES:
                    response = parseSubClasses(data);
                    break;
                case GET_DOMAIN_RELATIONS:
                    response = parseDomainRelations(data);
                    break;
                case GET_RANGE_RELATIONS:
                    response = parseRangeRelations(data);
                    break;
                case GET_CLASS_ATTRIBUTES:
                    response = parseClassAttributes(data);
                    break;
                case GET_CLASS_INDIVIDUALS:
                    response = parseClassIndividuals(data);
                    break;
                case GET_RELATION:
                    response = parseRelation(data);
                    break;
                case GET_RELATIONS:
                    response = parseRelations();
                    break;
                case GET_INVERSE_RELATION:
                    response = parseInverseRelation(data);
                    break;
                case GET_RELATION_DOMAINS:
                    response = parseRelationDomains(data);
                    break;
                case GET_RELATION_RANGES:
                    response = parseRelationRanges(data);
                    break;
                case GET_ATTRIBUTE:
                    response = parseAttribute(data);
                    break;
                case GET_ATTRIBUTES:
                    response = parseAttributes();
                    break;
                case GET_ATTRIBUTE_DOMAINS:
                    response = parseAttributeDomains(data);
                    break;
                case GET_INDIVIDUAL:
                    response = parseIndividual(data);
                    break;
                case GET_INDIVIDUALS:
                    response = parseIndividuals();
                    break;
                case GET_INDIVIDUAL_CLASSES:
                    response = parseIndividualClasses(data);
                    break;
                case GET_RELATION_VALUE:
                    response = parseRelationValue(data);
                    break;
                case GET_RELATION_VALUES:
                    response = parseRelationValues(data);
                    break;
                case GET_ATTRIBUTE_VALUES:
                    response = parseAttributeValues(data);
                    break;
                default:
                    response = new Response.Builder(FAIL).message("Unknown request query type \"" + gson.toJson(type, RequestType.class) + "\".").build();
            }

            return gson.toJson(response, Response.class);
        } catch (final Exception e) {
            LOGGER.log(Level.SEVERE, e.toString(), e);

            response = new Response.Builder(ERROR).message(e.toString()).build();

            return gson.toJson(response);
        }
    }

    @NotNull
    private Response parseSearchIndividuals(@NotNull final Map<String, Object> data) {
        final String keywords = (String) data.get("Keywords");
        final String unaccentKeywords = StringUtils.removeAccent(keywords);

        Set<Pair<Individual, Set<Pair<Attribute, List<String>>>>> searchIndividuals = null;
        for (final OWLNamedIndividual owlIndividual : ontology.getIndividualsInSignature()) {
            final String individualName = owlIndividual.getIRI().getShortForm();

            if (StringUtils.removeAccent(individualName).toLowerCase().contains(unaccentKeywords.toLowerCase())) {
                if (searchIndividuals == null) searchIndividuals = new HashSet<>();

                final Individual individual = getIndividual(owlIndividual);
                searchIndividuals.add(new Pair<>(individual, new HashSet<>()));
            }

            Set<Pair<Attribute, List<String>>> attributeValues = null;
            for (final OWLDataProperty owlAttribute : ontology.getDataPropertiesInSignature()) {
                final Set<OWLLiteral> owlLiterals = reasoner.getDataPropertyValues(owlIndividual, owlAttribute);
                if (owlLiterals.size() == 0) continue;

                Attribute key = null;
                List<String> values = null;
                for (final OWLLiteral owlLiteral : owlLiterals) {
                    final String literal = owlLiteral.getLiteral();
                    if (StringUtils.removeAccent(literal).toLowerCase().contains(unaccentKeywords.toLowerCase())) {
                        if (values == null) values = new ArrayList<>();
                        if (key == null) key = getAttribute(owlAttribute);
                        values.add(literal);
                    }
                }

                if (key != null) {
                    if (attributeValues == null) attributeValues = new HashSet<>();
                    if (!key.getId().equals("ten")) attributeValues.add(new Pair<>(key, values));
                }
            }

            if (attributeValues != null) {
                if (searchIndividuals == null) searchIndividuals = new HashSet<>();

                boolean addedIndividual = false;
                for (final Pair<Individual, Set<Pair<Attribute, List<String>>>> searchIndividual : searchIndividuals) {
                    if (searchIndividual.getLeft().getId().equals(individualName)) {
                        searchIndividuals.remove(searchIndividual);
                        searchIndividuals.add(new Pair<>(searchIndividual.getLeft(), attributeValues));

                        addedIndividual = true;
                        break;
                    }
                }
                if (!addedIndividual) {
                    final Individual individual = getIndividual(owlIndividual);
                    searchIndividuals.add(new Pair<>(individual, attributeValues));
                }
            }
        }

        final Response.Builder builder;
        if (searchIndividuals == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Search individuals \"" + keywords + "\" not found.");
        } else {
            builder = new Response.Builder(SUCCESS);
            builder.data("SearchIndividuals", searchIndividuals);
        }

        return builder.build();
    }

    @NotNull
    private Response parseClass(@NotNull final Map<String, Object> data) {
        final String className = (String) data.get("Class");

        final OWLClass owlClass = getOWLClass(className);

        final Response.Builder builder;
        if (owlClass == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Class \"" + className + "\" not found.");
        } else {
            final Class cls = getClass(owlClass);

            builder = new Response.Builder(SUCCESS);
            builder.data("Class", cls);
        }

        return builder.build();
    }

    @NotNull
    private Response parseSuperClasses(@NotNull final Map<String, Object> data) {
        final String className = (String) data.get("Class");
        final GetType getSuperClassType = gson.fromJson((String) data.get("GetSuperClassType"), GetType.class);

        final OWLClass owlClass = getOWLClass(className);

        final Response.Builder builder;
        if (owlClass == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Class \"" + className + "\" not found.");
        } else {
            final Set<Class> superClasses = getSuperClasses(owlClass, getSuperClassType);

            if (superClasses == null) {
                builder = new Response.Builder(FAIL);
                builder.message("Super classes of \"" + className + "\" not found.");
            } else {
                builder = new Response.Builder(SUCCESS);
                builder.data("SuperClasses", superClasses);
            }
        }

        return builder.build();
    }

    @NotNull
    private Response parseSubClasses(@NotNull final Map<String, Object> data) {
        final String className = (String) data.get("Class");
        final GetType getSubClassType = gson.fromJson((String) data.get("GetSubClassType"), GetType.class);

        final OWLClass owlClass = getOWLClass(className);

        final Response.Builder builder;
        if (owlClass == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Class \"" + className + "\" not found.");
        } else {
            final Set<Class> subClasses = getSubClasses(owlClass, getSubClassType);

            if (subClasses == null) {
                builder = new Response.Builder(FAIL);
                builder.message("Sub classes of \"" + className + "\" not found.");
            } else {
                builder = new Response.Builder(SUCCESS);
                builder.data("SubClasses", subClasses);
            }
        }

        return builder.build();
    }

    @NotNull
    private Response parseDomainRelations(@NotNull final Map<String, Object> data) {
        final String className = (String) data.get("Class");

        final OWLClass owlClass = getOWLClass(className);

        final Response.Builder builder;
        if (owlClass == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Class \"" + className + "\" not found.");
        } else {
            Set<Relation> relations = null;

            for (final OWLObjectProperty owlRelation : ontology.getObjectPropertiesInSignature()) {
                if (owlRelation.isOWLTopDataProperty()) continue;

                final Set<Class> domains = getRelationDomains(owlRelation, GET_ALL);
                if (domains != null)
                    for (final Class domain : domains)
                        if (domain.getId().equals(className)) {
                            final Relation relation = getRelation(owlRelation);

                            if (relations == null) relations = new HashSet<>();
                            relations.add(relation);
                            break;
                        }
            }

            if (relations == null) {
                builder = new Response.Builder(FAIL);
                builder.message("Relations of domain \"" + className + "\" not found.");
            } else {
                builder = new Response.Builder(SUCCESS);
                builder.data("DomainRelations", relations);
            }
        }

        return builder.build();
    }

    @NotNull
    private Response parseRangeRelations(@NotNull final Map<String, Object> data) {
        final String className = (String) data.get("Class");

        final OWLClass owlClass = getOWLClass(className);

        final Response.Builder builder;
        if (owlClass == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Class \"" + className + "\" not found.");
        } else {
            Set<Relation> relations = null;

            for (final OWLObjectProperty owlRelation : ontology.getObjectPropertiesInSignature()) {
                if (owlRelation.isOWLTopDataProperty()) continue;

                final Set<Class> ranges = getRelationRanges(owlRelation, GET_ALL);
                if (ranges != null)
                    for (final Class range : ranges)
                        if (range.getId().equals(className)) {
                            final Relation relation = getRelation(owlRelation);

                            if (relations == null) relations = new HashSet<>();
                            relations.add(relation);
                            break;
                        }
            }

            if (relations == null) {
                builder = new Response.Builder(FAIL);
                builder.message("Relations of range \"" + className + "\" not found.");
            } else {
                builder = new Response.Builder(SUCCESS);
                builder.data("RangeRelations", relations);
            }
        }

        return builder.build();
    }

    @NotNull
    private Response parseClassAttributes(@NotNull final Map<String, Object> data) {
        final String className = (String) data.get("Class");

        final OWLClass owlClass = getOWLClass(className);

        final Response.Builder builder;
        if (owlClass == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Class \"" + className + "\" not found.");
        } else {
            final Set<Attribute> attributes = getClassAttributes(owlClass);

            if (attributes == null) {
                builder = new Response.Builder(FAIL);
                builder.message("Attributes of \"" + className + "\" not found.");
            } else {
                builder = new Response.Builder(SUCCESS);
                builder.data("ClassAttributes", attributes);
            }
        }

        return builder.build();
    }

    @NotNull
    private Response parseClassIndividuals(@NotNull final Map<String, Object> data) {
        final String className = (String) data.get("Class");
        final GetType getIndividualType = gson.fromJson((String) data.get("GetIndividualType"), GetType.class);

        final OWLClass owlClass = getOWLClass(className);

        final Response.Builder builder;
        if (owlClass == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Class \"" + className + "\" not found.");
        } else {
            final Set<Individual> individuals = getClassIndividuals(owlClass, getIndividualType);

            if (individuals == null) {
                builder = new Response.Builder(FAIL);
                builder.message("Individuals of \"" + className + "\" not found.");
            } else {
                builder = new Response.Builder(SUCCESS);
                builder.data("ClassIndividuals", individuals);
            }
        }

        return builder.build();
    }

    @NotNull
    private Response parseRelation(@NotNull final Map<String, Object> data) {
        final String relationName = (String) data.get("Relation");

        final OWLObjectProperty owlRelation = getOWLRelation(relationName);

        final Response.Builder builder;
        if (owlRelation == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Relation \"" + relationName + "\" not found.");
        } else {
            final Relation relation = getRelation(owlRelation);

            builder = new Response.Builder(SUCCESS);
            builder.data("Relation", relation);
        }

        return builder.build();
    }

    @NotNull
    private Response parseRelations() {
        Set<Relation> relations = null;

        for (final OWLObjectProperty owlRelation : ontology.getObjectPropertiesInSignature()) {
            if (owlRelation.isOWLTopDataProperty()) continue;

            final Relation relation = getRelation(owlRelation);

            if (relations == null) relations = new HashSet<>();
            relations.add(relation);
        }

        final Response.Builder builder;
        if (relations == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Relations not found.");
        } else {
            builder = new Response.Builder(SUCCESS);
            builder.data("Relations", relations);
        }

        return builder.build();
    }

    @NotNull
    private Response parseInverseRelation(@NotNull final Map<String, Object> data) {
        final String relationName = (String) data.get("Relation");

        final OWLObjectProperty owlRelation = getOWLRelation(relationName);

        final Response.Builder builder;
        if (owlRelation == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Relation \"" + relationName + "\" not found.");
        } else {
            final OWLObjectProperty owlInverseRelation = getOWLInverseRelation(owlRelation);

            if (owlInverseRelation == null) {
                builder = new Response.Builder(FAIL);
                builder.message("Inverse relation of \"" + relationName + "\" not found.");
            } else {
                final Relation relation = getRelation(owlRelation);

                builder = new Response.Builder(SUCCESS);
                builder.data("InverseRelation", relation);
            }
        }

        return builder.build();
    }

    @NotNull
    private Response parseRelationDomains(@NotNull final Map<String, Object> data) {
        final String relationName = (String) data.get("Relation");
        final GetType getDomainType = gson.fromJson((String) data.get("GetDomainType"), GetType.class);

        final OWLObjectProperty owlRelation = getOWLRelation(relationName);

        final Response.Builder builder;
        if (owlRelation == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Relation \"" + relationName + "\" not found.");
        } else {
            final Set<Class> domains = getRelationDomains(owlRelation, getDomainType);

            if (domains == null) {
                builder = new Response.Builder(FAIL);
                builder.message("Domains of \"" + relationName + "\" not found.");
            } else {
                builder = new Response.Builder(SUCCESS);
                builder.data("RelationDomains", domains);
            }
        }

        return builder.build();
    }

    @NotNull
    private Response parseRelationRanges(@NotNull final Map<String, Object> data) {
        final String relationName = (String) data.get("Relation");
        final GetType getRangeType = gson.fromJson((String) data.get("GetRangeType"), GetType.class);

        final OWLObjectProperty owlRelation = getOWLRelation(relationName);

        final Response.Builder builder;
        if (owlRelation == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Relation \"" + relationName + "\" not found.");
        } else {
            final Set<Class> ranges = getRelationRanges(owlRelation, getRangeType);

            if (ranges == null) {
                builder = new Response.Builder(FAIL);
                builder.message("Ranges of \"" + relationName + "\" not found.");
            } else {
                builder = new Response.Builder(SUCCESS);
                builder.data("RelationRanges", ranges);
            }
        }

        return builder.build();
    }

    @NotNull
    private Response parseAttribute(@NotNull final Map<String, Object> data) {
        final String attributeName = (String) data.get("Attribute");

        final OWLDataProperty owlAttribute = getOWLAttribute(attributeName);

        final Response.Builder builder;
        if (owlAttribute == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Attribute \"" + attributeName + "\" not found.");
        } else {
            final Attribute attribute = getAttribute(owlAttribute);

            builder = new Response.Builder(SUCCESS);
            builder.data("Attribute", attribute);
        }

        return builder.build();
    }

    @NotNull
    private Response parseAttributes() {
        Set<Attribute> attributes = null;

        for (final OWLDataProperty owlAttribute : ontology.getDataPropertiesInSignature()) {
            if (owlAttribute.isOWLTopDataProperty()) continue;

            final Attribute attribute = getAttribute(owlAttribute);

            if (attributes == null) attributes = new HashSet<>();
            attributes.add(attribute);
        }

        final Response.Builder builder;
        if (attributes == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Attributes not found.");
        } else {
            builder = new Response.Builder(SUCCESS);
            builder.data("Attributes", attributes);
        }

        return builder.build();
    }

    @NotNull
    private Response parseAttributeDomains(@NotNull final Map<String, Object> data) {
        final String attributeName = (String) data.get("Attribute");
        final GetType getDomainType = gson.fromJson((String) data.get("GetDomainType"), GetType.class);

        final OWLDataProperty owlAttribute = getOWLAttribute(attributeName);

        final Response.Builder builder;
        if (owlAttribute == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Attribute \"" + attributeName + "\" not found.");
        } else {
            final Set<Class> domains = getAttributeDomains(owlAttribute, getDomainType);

            if (domains == null) {
                builder = new Response.Builder(FAIL);
                builder.message("Domains of \"" + attributeName + "\" not found.");
            } else {
                builder = new Response.Builder(SUCCESS);
                builder.data("AttributeDomains", domains);
            }
        }

        return builder.build();
    }

    @NotNull
    private Response parseIndividual(@NotNull final Map<String, Object> data) {
        final String individualName = (String) data.get("Individual");

        final OWLNamedIndividual owlIndividual = getOWLIndividual(individualName);

        final Response.Builder builder;
        if (owlIndividual == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Individual \"" + individualName + "\" not found.");
        } else {
            final Individual individual = getIndividual(owlIndividual);

            builder = new Response.Builder(SUCCESS);
            builder.data("Individual", individual);
        }

        return builder.build();
    }

    @NotNull
    private Response parseIndividuals() {
        Set<Individual> individuals = null;

        for (final OWLNamedIndividual owlIndividual : ontology.getIndividualsInSignature()) {
            final Individual individual = getIndividual(owlIndividual);

            if (individuals == null) individuals = new HashSet<>();
            individuals.add(individual);
        }

        final Response.Builder builder;
        if (individuals == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Individuals not found.");
        } else {
            builder = new Response.Builder(SUCCESS);
            builder.data("Individuals", individuals);
        }

        return builder.build();
    }

    @NotNull
    private Response parseIndividualClasses(@NotNull final Map<String, Object> data) {
        final String individualName = (String) data.get("Individual");
        final GetType getClassType = gson.fromJson((String) data.get("GetClassType"), GetType.class);

        final OWLNamedIndividual owlIndividual = getOWLIndividual(individualName);

        final Response.Builder builder;
        if (owlIndividual == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Individual \"" + individualName + "\" not found.");
        } else {
            final Set<Class> classes = getIndividualClasses(owlIndividual, getClassType);

            if (classes == null) {
                builder = new Response.Builder(FAIL);
                builder.message("Classes of \"" + individualName + "\" not found.");
            } else {
                builder = new Response.Builder(SUCCESS);
                if (getClassType == GET_DIRECT)
                    builder.data("IndividualClass", classes.iterator().next());
                else builder.data("IndividualClasses", classes);
            }
        }

        return builder.build();
    }

    @NotNull
    private Response parseRelationValue(@NotNull final Map<String, Object> data) {
        final String individualName = (String) data.get("Individual");
        final String relationName = (String) data.get("Relation");

        final OWLNamedIndividual owlIndividual = getOWLIndividual(individualName);
        final OWLObjectProperty owlRelation = getOWLRelation(relationName);

        final Response.Builder builder;
        if (owlIndividual == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Individual \"" + individualName + "\" not found.");
        } else if (owlRelation == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Relation \"" + relationName + "\" not found.");
        } else {
            final List<Individual> relationValue = getRelationValue(owlIndividual, owlRelation);

            if (relationValue == null) {
                builder = new Response.Builder(FAIL);
                builder.message("Relation value \"" + relationName + "\" of \"" + individualName + "\" not found.");
            } else {
                builder = new Response.Builder(SUCCESS);
                builder.data("RelationValue", relationValue);
            }
        }

        return builder.build();
    }

    @NotNull
    private Response parseRelationValues(@NotNull final Map<String, Object> data) {
        final String individualName = (String) data.get("Individual");

        final OWLNamedIndividual owlIndividual = getOWLIndividual(individualName);

        final Response.Builder builder;
        if (owlIndividual == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Individual \"" + individualName + "\" not found.");
        } else {
            final Set<Pair<Relation, List<Individual>>> relationValues = getRelationValues(owlIndividual);

            if (relationValues == null) {
                builder = new Response.Builder(FAIL);
                builder.message("Relation values of \"" + individualName + "\" not found.");
            } else {
                builder = new Response.Builder(SUCCESS);
                builder.data("RelationValues", relationValues);
            }
        }

        return builder.build();
    }

    @NotNull
    private Response parseAttributeValues(@NotNull final Map<String, Object> data) {
        final String individualName = (String) data.get("Individual");

        final OWLNamedIndividual owlIndividual = getOWLIndividual(individualName);

        final Response.Builder builder;
        if (owlIndividual == null) {
            builder = new Response.Builder(FAIL);
            builder.message("Individual \"" + individualName + "\" not found.");
        } else {
            final Set<Pair<Attribute, List<String>>> attributeValues = getAttributeValues(owlIndividual);

            if (attributeValues == null) {
                builder = new Response.Builder(FAIL);
                builder.message("Attributes values of \"" + individualName + "\" not found.");
            } else {
                builder = new Response.Builder(SUCCESS);
                builder.data("AttributeValues", attributeValues);
            }
        }

        return builder.build();
    }

    @NotNull
    private Class getClass(@NotNull final OWLClass owlClass) {
        final String classLabel = getLabel(owlClass);
        final Class.Builder builder = new Class.Builder(owlClass.getIRI().getShortForm(), classLabel);

        return builder.build();
    }

    @Nullable
    private Set<Class> getSuperClasses(@NotNull final OWLClass owlClass, @NotNull final GetType getSuperClassType) {
        Set<Class> superClasses = null;

        final Set<OWLClass> owlSuperClasses = reasoner.getSuperClasses(owlClass, getSuperClassType == GET_DIRECT).getFlattened();
        for (final OWLClass owlSuperClass : owlSuperClasses) {

            final String superClassName = owlSuperClass.getIRI().getShortForm();
            final String superClassLabel = getLabel(owlSuperClass);
            final Class superClass = new Class.Builder(superClassName, superClassLabel).build();

            if (superClasses == null) superClasses = new HashSet<>();
            superClasses.add(superClass);
        }

        return superClasses;
    }

    @Nullable
    private Set<Class> getSubClasses(@NotNull final OWLClass owlClass, @NotNull final GetType getSubClassType) {
        Set<Class> superClasses = null;

        final Set<OWLClass> owlSubClasses = reasoner.getSubClasses(owlClass, getSubClassType == GET_DIRECT).getFlattened();
        for (final OWLClass owlSubClass : owlSubClasses) {
            if (owlSubClass.isOWLNothing()) continue;

            final String subClassName = owlSubClass.getIRI().getShortForm();
            final String subClassLabel = getLabel(owlSubClass);
            final Class subClass = new Class.Builder(subClassName, subClassLabel).build();

            if (superClasses == null) superClasses = new HashSet<>();
            superClasses.add(subClass);
        }

        return superClasses;
    }

    @Nullable
    private Set<Attribute> getClassAttributes(@NotNull final OWLClass owlClass) {
        Set<Attribute> attributes = null;

        for (final OWLDataProperty owlAttribute : ontology.getDataPropertiesInSignature()) {
            if (owlAttribute.isOWLTopDataProperty()) continue;

            final Set<Class> domains = getAttributeDomains(owlAttribute, GET_ALL);
            if (domains == null) continue;

            for (final Class domain : domains) {
                if (domain.getId().equals(owlClass.getIRI().getShortForm())) {

                    final Attribute attribute = getAttribute(owlAttribute);

                    if (attributes == null) attributes = new HashSet<>();
                    attributes.add(attribute);

                    break;
                }
            }
        }

        return attributes;
    }

    @Nullable
    private Set<Individual> getClassIndividuals(@NotNull final OWLClass owlClass, @NotNull final GetType getIndividualType) {
        Set<Individual> individuals = null;

        final Set<OWLNamedIndividual> owlIndividuals = reasoner.getInstances(owlClass, getIndividualType == GET_DIRECT).getFlattened();
        for (final OWLNamedIndividual owlIndividual : owlIndividuals) {
            final Individual individual = getIndividual(owlIndividual);

            if (individuals == null) individuals = new HashSet<>();
            individuals.add(individual);
        }

        return individuals;
    }

    @NotNull
    private Relation getRelation(@NotNull final OWLObjectProperty owlRelation) {
        final String relationLabel = getLabel(owlRelation);
        final Relation.Builder builder = new Relation.Builder(owlRelation.getIRI().getShortForm(), relationLabel);

        return builder.build();
    }

    @Nullable
    private Set<Class> getRelationDomains(@NotNull final OWLObjectProperty owlRelation, @NotNull final GetType getDomainType) {
        final Set<OWLObjectPropertyDomainAxiom> owlAxioms = ontology.getObjectPropertyDomainAxioms(owlRelation);
        if (owlAxioms.size() != 1) return null;

        final OWLClassExpression owlDomainExpression = owlAxioms.iterator().next().getDomain();
        Set<OWLClass> owlDomains = owlDomainExpression.getClassesInSignature();

        Set<Class> domains = null;
        if (owlDomains.size() == 1) {
            if (getDomainType == GET_ALL) {
                final OWLClass owlSuperDomain = owlDomains.iterator().next();
                final Class domain = getClass(owlSuperDomain);

                domains = new HashSet<>();
                domains.add(domain);

                owlDomains = reasoner.getSubClasses(owlSuperDomain, false).getFlattened();
            }
        } else owlDomains = reasoner.getSubClasses(owlDomainExpression, getDomainType == GET_DIRECT).getFlattened();

        for (final OWLClass owlDomain : owlDomains) {
            if (owlDomain.isOWLNothing()) continue;

            final Class domain = getClass(owlDomain);

            if (domains == null) domains = new HashSet<>();
            domains.add(domain);
        }

        return domains;
    }

    @Nullable
    private Set<Class> getRelationRanges(@NotNull final OWLObjectProperty owlRelation, @NotNull final GetType getRangeType) {
        final Set<OWLObjectPropertyRangeAxiom> owlAxioms = ontology.getObjectPropertyRangeAxioms(owlRelation);
        if (owlAxioms.size() != 1) return null;

        final OWLClassExpression owlRangeExpression = owlAxioms.iterator().next().getRange();
        Set<OWLClass> owlRanges = owlRangeExpression.getClassesInSignature();

        Set<Class> ranges = null;
        if (owlRanges.size() == 1) {
            if (getRangeType == GET_ALL) {
                final OWLClass owlSuperRange = owlRanges.iterator().next();
                final Class range = getClass(owlSuperRange);

                ranges = new HashSet<>();
                ranges.add(range);

                owlRanges = reasoner.getSubClasses(owlSuperRange, false).getFlattened();
            }
        } else owlRanges = reasoner.getSubClasses(owlRangeExpression, getRangeType == GET_DIRECT).getFlattened();

        for (final OWLClass owlRange : owlRanges) {
            if (owlRange.isOWLNothing()) continue;

            final Class range = getClass(owlRange);

            if (ranges == null) ranges = new HashSet<>();
            ranges.add(range);
        }

        return ranges;
    }

    @NotNull
    private Attribute getAttribute(@NotNull final OWLDataProperty owlAttribute) {
        final String attributeLabel = getLabel(owlAttribute);
        final Attribute.Builder builder = new Attribute.Builder(owlAttribute.getIRI().getShortForm(), attributeLabel);

        final Pair<DataType, List<String>> pair = getAttributeRange(owlAttribute);
        if (pair != null) {
            if (pair.getLeft() == ENUMERATED) builder.enumeratedValues(pair.getRight());
            else builder.range(pair.getLeft());
        }

        return builder.build();
    }

    @Nullable
    private Set<Class> getAttributeDomains(@NotNull final OWLDataProperty owlAttribute, @NotNull final GetType getDomainType) {
        final Set<OWLDataPropertyDomainAxiom> owlAxioms = ontology.getDataPropertyDomainAxioms(owlAttribute);
        if (owlAxioms.size() != 1) return null;

        final OWLClassExpression owlDomainExpression = owlAxioms.iterator().next().getDomain();
        Set<OWLClass> owlDomains = owlDomainExpression.getClassesInSignature();

        Set<Class> domains = null;
        if (owlDomains.size() == 1) {
            if (getDomainType == GET_ALL) {
                final OWLClass owlSuperDomain = owlDomains.iterator().next();
                final Class domain = getClass(owlSuperDomain);

                domains = new HashSet<>();
                domains.add(domain);

                owlDomains = reasoner.getSubClasses(owlSuperDomain, false).getFlattened();
            }
        } else owlDomains = reasoner.getSubClasses(owlDomainExpression, getDomainType == GET_DIRECT).getFlattened();

        for (final OWLClass owlDomain : owlDomains) {
            if (owlDomain.isOWLNothing()) continue;

            final Class domain = getClass(owlDomain);

            if (domains == null) domains = new HashSet<>();
            domains.add(domain);
        }

        return domains;
    }

    @Nullable
    private Pair<DataType, List<String>> getAttributeRange(@NotNull final OWLDataProperty owlAttribute) {
        final Set<OWLDataPropertyRangeAxiom> owlAxioms = ontology.getDataPropertyRangeAxioms(owlAttribute);
        if (owlAxioms.size() != 1) return null;

        final OWLDataRange owlDataRange = owlAxioms.iterator().next().getRange();
        if (owlDataRange.isDatatype()) {
            final OWLDatatype owlDataType = owlDataRange.asOWLDatatype();

            final DataType range;
            if (owlDataType.isString()) range = STRING;
            else if (owlDataType.isBoolean()) range = BOOLEAN;
            else {
                final String owlDataTypeName = owlDataType.getIRI().getShortForm();

                if (owlDataTypeName.equals("int")) range = INT;
                else range = UNKNOWN;
            }

            return new Pair<>(range, null);
        } else return new Pair<>(UNKNOWN, null);
    }

    @NotNull
    private Individual getIndividual(@NotNull final OWLNamedIndividual owlIndividual) {
        String individualLabel = null;

        final OWLDataProperty owlAttribute = factory.getOWLDataProperty(IRI.create(prefix, "ten"));
        if (ontology.containsDataPropertyInSignature(owlAttribute.getIRI())) {
            final Set<OWLLiteral> owlLiterals = reasoner.getDataPropertyValues(owlIndividual, owlAttribute);
            if (owlLiterals.size() != 0) individualLabel = owlLiterals.iterator().next().getLiteral();
        }

        final Individual.Builder builder = new Individual.Builder(owlIndividual.getIRI().getShortForm(), individualLabel);

        return builder.build();
    }

    @Nullable
    private Set<Class> getIndividualClasses(@NotNull final OWLNamedIndividual owlIndividual, @NotNull final GetType getClassType) {
        final Set<OWLClassAssertionAxiom> owlAxioms = ontology.getClassAssertionAxioms(owlIndividual);
        if (owlAxioms.size() != 1) return null;

        final Set<Class> classes = new HashSet<>();

        final Set<OWLClass> owlClasses;
        if (getClassType == GET_DIRECT) {
            owlClasses = owlAxioms.iterator().next().getClassesInSignature();
            if (owlClasses.size() != 1) return null;
        } else owlClasses = reasoner.getTypes(owlIndividual, false).getFlattened();

        for (final OWLClass owlClass : owlClasses) {
            if (owlClass.isOWLThing()) continue;

            final String className = owlClass.getIRI().getShortForm();
            final String classLabel = getLabel(owlClass);
            final Class cls = new Class.Builder(className, classLabel).build();

            classes.add(cls);
        }

        return classes;
    }

    @Nullable
    private Set<Pair<Relation, List<Individual>>> getRelationValues(@NotNull final OWLNamedIndividual owlIndividual) {
        Set<Pair<Relation, List<Individual>>> relationValues = null;

        for (final OWLObjectProperty owlRelation : ontology.getObjectPropertiesInSignature()) {
            final Set<OWLNamedIndividual> owlIndividualValues = reasoner.getObjectPropertyValues(owlIndividual, owlRelation).getFlattened();
            if (owlIndividualValues.size() == 0) continue;

            final Relation key = getRelation(owlRelation);

            final List<Individual> values = new ArrayList<>();
            for (final OWLNamedIndividual owlIndividualValue : owlIndividualValues) {
                final Individual individual = getIndividual(owlIndividualValue);

                values.add(individual);
            }

            if (relationValues == null) relationValues = new HashSet<>();
            relationValues.add(new Pair<>(key, values));
        }

        return relationValues;
    }

    @Nullable
    private List<Individual> getRelationValue(@NotNull final OWLNamedIndividual owlIndividual, @NotNull final OWLObjectProperty owlRelation) {

        final Set<OWLNamedIndividual> owlIndividualValues = reasoner.getObjectPropertyValues(owlIndividual, owlRelation).getFlattened();
        if (owlIndividualValues.size() == 0) return null;

        final List<Individual> values = new ArrayList<>();
        for (final OWLNamedIndividual owlIndividualValue : owlIndividualValues) {
            final Individual individual = getIndividual(owlIndividualValue);

            values.add(individual);
        }

        return values;
    }

    @Nullable
    private Set<Pair<Attribute, List<String>>> getAttributeValues(@NotNull final OWLNamedIndividual owlIndividual) {
        Set<Pair<Attribute, List<String>>> attributeValues = null;

        for (final OWLDataProperty owlAttribute : ontology.getDataPropertiesInSignature()) {
            final Set<OWLLiteral> owlLiterals = reasoner.getDataPropertyValues(owlIndividual, owlAttribute);
            if (owlLiterals.size() == 0) continue;

            final Attribute key = getAttribute(owlAttribute);

            final List<String> values = new ArrayList<>();
            for (final OWLLiteral owlLiteral : owlLiterals) {
                values.add(owlLiteral.getLiteral());
            }

            if (attributeValues == null) attributeValues = new HashSet<>();
            attributeValues.add(new Pair<>(key, values));
        }

        return attributeValues;
    }

    @Nullable
    private String getLabel(@NotNull final OWLNamedObject owlObject) {
        final OWLAnnotationProperty rdfsLabel = factory.getOWLAnnotationProperty(OWLRDFVocabulary.RDFS_LABEL.getIRI());

        String label = null;
        for (final OWLAnnotation annotation : Searcher.annotationObjects(ontology.getAnnotationAssertionAxioms(owlObject.getIRI()), rdfsLabel)) {
            if (annotation.getValue() instanceof OWLLiteral) {
                final OWLLiteral value = (OWLLiteral) annotation.getValue();
                label = value.getLiteral();
                if (value.hasLang(DEFAULT_LANG)) break;
            }
        }

        return label;
    }

    @Nullable
    private OWLClass getOWLClass(@NotNull final String className) {
        final OWLClass owlClass;

        if (className.equals("Thing")) owlClass = factory.getOWLThing();
        else {
            owlClass = factory.getOWLClass(IRI.create(prefix, className));
            if (!ontology.containsClassInSignature(owlClass.getIRI())) return null;
        }

        return owlClass;
    }

    @Nullable
    private OWLObjectProperty getOWLInverseRelation(@NotNull final OWLObjectProperty owlRelation) {
        final Set<OWLObjectPropertyExpression> owlInverseRelations = reasoner.getInverseObjectProperties(owlRelation).getEntities();

        if (owlInverseRelations.size() == 2) {
            final Iterator<OWLObjectPropertyExpression> owlInverseRelationIterator = owlInverseRelations.iterator();
            final OWLObjectPropertyExpression owlInverseRelation = owlInverseRelationIterator.next();
            if (!owlInverseRelation.getNamedProperty().getIRI().getShortForm().equals(owlRelation.getIRI().getShortForm()))
                return owlInverseRelation.asOWLObjectProperty();
            else {
                return owlInverseRelationIterator.next().asOWLObjectProperty();
            }
        } else return null;
    }

    @Nullable
    private OWLObjectProperty getOWLRelation(@NotNull final String relationName) {
        final OWLObjectProperty owlRelation = factory.getOWLObjectProperty(IRI.create(prefix, relationName));
        if (!ontology.containsObjectPropertyInSignature(owlRelation.getIRI())) return null;

        return owlRelation;
    }

    @Nullable
    private OWLDataProperty getOWLAttribute(@NotNull final String attributeName) {
        final OWLDataProperty owlAttribute = factory.getOWLDataProperty(IRI.create(prefix, attributeName));
        if (!ontology.containsDataPropertyInSignature(owlAttribute.getIRI())) return null;

        return owlAttribute;
    }

    @Nullable
    private OWLNamedIndividual getOWLIndividual(@NotNull final String individualName) {
        final OWLNamedIndividual owlIndividual = factory.getOWLNamedIndividual(IRI.create(prefix, individualName));
        if (!ontology.containsIndividualInSignature(owlIndividual.getIRI())) return null;

        return owlIndividual;
    }
}
