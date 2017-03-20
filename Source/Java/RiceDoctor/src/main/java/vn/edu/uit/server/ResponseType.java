package vn.edu.uit.server;

import com.google.gson.annotations.SerializedName;

public enum ResponseType {

    @SerializedName("Success")
    SUCCESS,

    @SerializedName("Fail")
    FAIL,

    @SerializedName("Error")
    ERROR
}
