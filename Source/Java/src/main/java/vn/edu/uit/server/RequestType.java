package vn.edu.uit.server;

import com.google.gson.annotations.SerializedName;

public enum RequestType {
    @SerializedName("GetComment")
    GET_COMMENT,

    @SerializedName("GetClass")
    GET_CLASS,

    @SerializedName("GetSuperClasses")
    GET_SUPERCLASSES,

    @SerializedName("GetSubClasses")
    GET_SUBCLASSES,

    @SerializedName("GetDomainRelations")
    GET_DOMAIN_RELATIONS,

    @SerializedName("GetRangeRelations")
    GET_RANGE_RELATIONS,

    @SerializedName("GetClassAttributes")
    GET_CLASS_ATTRIBUTES,

    @SerializedName("GetClassIndividuals")
    GET_CLASS_INDIVIDUALS,

    @SerializedName("GetRelation")
    GET_RELATION,

    @SerializedName("GetRelations")
    GET_RELATIONS,

    @SerializedName("GetInverseRelation")
    GET_INVERSE_RELATION,

    @SerializedName("GetRelationDomains")
    GET_RELATION_DOMAINS,

    @SerializedName("GetRelationRanges")
    GET_RELATION_RANGES,

    @SerializedName("GetAttribute")
    GET_ATTRIBUTE,

    @SerializedName("GetAttributes")
    GET_ATTRIBUTES,

    @SerializedName("GetAttributeDomains")
    GET_ATTRIBUTE_DOMAINS,

    @SerializedName("GetIndividual")
    GET_INDIVIDUAL,

    @SerializedName("GetIndividuals")
    GET_INDIVIDUALS,

    @SerializedName("GetIndividualClasses")
    GET_INDIVIDUAL_CLASSES,

    @SerializedName("GetIndividualNames")
    GET_INDIVIDUAL_NAMES,

    @SerializedName("GetRelationValue")
    GET_RELATION_VALUE,

    @SerializedName("GetRelationValues")
    GET_RELATION_VALUES,

    @SerializedName("GetAttributeValues")
    GET_ATTRIBUTE_VALUES
}
