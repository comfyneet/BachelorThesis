package vn.edu.uit.server;

import com.google.gson.annotations.SerializedName;
import org.jetbrains.annotations.NotNull;
import org.jetbrains.annotations.Nullable;
import vn.edu.uit.shared.Buildable;

import java.util.HashMap;
import java.util.Map;

public class Request {

    @NotNull
    @SerializedName("Type")
    private final RequestType type;

    @Nullable
    @SerializedName("Data")
    private final Map<String, Object> data;

    private Request(@NotNull final Builder builder) {
        this.type = builder.type;
        this.data = builder.data;
    }

    @NotNull
    public final RequestType getType() {
        return type;
    }

    @Nullable
    public final Map<String, Object> getData() {
        return data;
    }

    public class Builder implements Buildable<Request> {

        @NotNull
        private final RequestType type;

        @Nullable
        private Map<String, Object> data;

        public Builder(@NotNull final RequestType type) {
            this.type = type;
        }

        @NotNull
        public Builder data(@NotNull final String key, @Nullable final Object value) {
            if (data == null) data = new HashMap<>();
            data.put(key, value);
            return this;
        }

        @Override
        @NotNull
        public Request build() {
            return new Request(this);
        }
    }
}
