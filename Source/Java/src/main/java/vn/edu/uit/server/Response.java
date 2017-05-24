package vn.edu.uit.server;

import com.google.gson.annotations.SerializedName;
import org.jetbrains.annotations.NotNull;
import org.jetbrains.annotations.Nullable;
import vn.edu.uit.shared.Buildable;

import java.util.HashMap;
import java.util.Map;

public class Response {

    @NotNull
    @SerializedName("Status")
    private final ResponseType status;

    @Nullable
    @SerializedName("Data")
    private final Map<String, Object> data;

    @Nullable
    @SerializedName("Message")
    private final String message;

    Response(@NotNull final Builder builder) {
        status = builder.status;
        data = builder.data;
        message = builder.message;
    }

    public static class Builder implements Buildable<Response> {

        @NotNull
        private final ResponseType status;

        @Nullable
        private Map<String, Object> data;

        @Nullable
        private String message;

        public Builder(@NotNull final ResponseType status) {
            this.status = status;
        }

        @NotNull
        public Builder data(@NotNull final String key, @Nullable final Object value) {
            if (data == null) data = new HashMap<>();
            data.put(key, value);
            return this;
        }

        @NotNull
        public Builder message(@NotNull final String message) {
            this.message = message;
            return this;
        }

        @Override
        @NotNull
        public Response build() {
            return new Response(this);
        }
    }
}
