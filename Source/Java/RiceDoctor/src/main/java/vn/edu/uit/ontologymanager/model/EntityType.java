package vn.edu.uit.ontologymanager.model;

import com.google.gson.annotations.SerializedName;

public enum EntityType {
    @SerializedName("Class")
    CLASS,

    @SerializedName("Relation")
    RELATION,

    @SerializedName("Attribute")
    ATTRIBUTE,

    @SerializedName("Individual")
    INDIVIDUAL
}
