package vn.edu.uit.ontologymanager.model;

import com.google.gson.annotations.SerializedName;

public enum DataType {

    @SerializedName("Unknown")
    UNKNOWN,

    @SerializedName("Int")
    INT,

    @SerializedName("String")
    STRING,

    @SerializedName("Boolean")
    BOOLEAN
}
