package vn.edu.uit.ontologymanager.model;

import com.google.gson.annotations.SerializedName;
import org.jetbrains.annotations.NotNull;
import org.jetbrains.annotations.Nullable;

public abstract class Entity {

    @NotNull
    @SerializedName("Id")
    private final String id;

    @Nullable
    @SerializedName("Label")
    private final String label;

    @NotNull
    @SerializedName("Type")
    private final EntityType type;

    Entity(@NotNull final EntityType type, @NotNull final String id, @Nullable final String label) {
        this.type = type;
        this.id = id;
        this.label = label;
    }

    @NotNull
    public final String getId() {
        return id;
    }
}
