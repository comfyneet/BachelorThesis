package vn.edu.uit.ontologymanager.model;

import com.google.gson.annotations.SerializedName;
import org.jetbrains.annotations.NotNull;
import org.jetbrains.annotations.Nullable;
import vn.edu.uit.shared.Buildable;
import vn.edu.uit.shared.Pair;

import java.util.List;
import java.util.Set;

import static vn.edu.uit.ontologymanager.model.EntityType.INDIVIDUAL;

public class Individual extends Entity {

    @Nullable
    @SerializedName("DirectClass")
    private final Class directClass;

    @Nullable
    @SerializedName("AllClasses")
    private final Set<Class> allClasses;

    @Nullable
    @SerializedName("RelationValues")
    private final Set<Pair<Relation, List<Individual>>> relationValues;

    @Nullable
    @SerializedName("AttributeValues")
    private final Set<Pair<Attribute, List<String>>> attributeValues;

    Individual(@NotNull final Builder builder) {
        super(INDIVIDUAL, builder.id, builder.label);
        directClass = builder.directClass;
        allClasses = builder.allClasses;
        relationValues = builder.relationValues;
        attributeValues = builder.attributeValues;
    }

    public static class Builder implements Buildable<Individual> {

        @NotNull
        private final String id;

        @Nullable
        private final String label;

        @Nullable
        private Class directClass;

        @Nullable
        private Set<Class> allClasses;

        @Nullable
        private Set<Pair<Relation, List<Individual>>> relationValues;

        @Nullable
        private Set<Pair<Attribute, List<String>>> attributeValues;

        public Builder(@NotNull final String id, @Nullable final String label) {
            this.id = id;
            this.label = label;
        }

        @NotNull
        public Builder directClass(@Nullable final Class directClass) {
            this.directClass = directClass;
            return this;
        }

        @NotNull
        public Builder allClasses(@Nullable final Set<Class> allClasses) {
            this.allClasses = allClasses;
            return this;
        }

        @NotNull
        public Builder relationValues(@Nullable final Set<Pair<Relation, List<Individual>>> relationValues) {
            this.relationValues = relationValues;
            return this;
        }

        @NotNull
        public Builder attributeValues(@Nullable final Set<Pair<Attribute, List<String>>> attributeValues) {
            this.attributeValues = attributeValues;
            return this;
        }

        @Override
        @NotNull
        public Individual build() {
            return new Individual(this);
        }
    }
}