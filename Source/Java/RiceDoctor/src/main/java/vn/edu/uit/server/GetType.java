package vn.edu.uit.server;

import com.google.gson.annotations.SerializedName;

public enum GetType {
    @SerializedName("GetDirect")
    GET_DIRECT,

    @SerializedName("GetAll")
    GET_ALL
}
