package vn.edu.uit.ontologymanager.model;

import com.google.gson.annotations.SerializedName;
import org.jetbrains.annotations.NotNull;
import org.jetbrains.annotations.Nullable;
import vn.edu.uit.shared.Buildable;

import java.util.Set;

import static vn.edu.uit.ontologymanager.model.EntityType.RELATION;

public class Relation extends Entity {

    @Nullable
    @SerializedName("DirectDomains")
    private final Set<Class> directDomains;

    @Nullable
    @SerializedName("AllDomains")
    private final Set<Class> allDomains;

    @Nullable
    @SerializedName("DirectRanges")
    private final Set<Class> directRanges;

    @Nullable
    @SerializedName("AllRanges")
    private final Set<Class> allRanges;

    Relation(@NotNull final Builder builder) {
        super(RELATION, builder.id, builder.label);
        directDomains = builder.directDomains;
        allDomains = builder.allDomains;
        directRanges = builder.directRanges;
        allRanges = builder.allRanges;
    }

    public static class Builder implements Buildable<Relation> {

        @NotNull
        private final String id;

        @Nullable
        private final String label;

        @Nullable
        private Set<Class> directDomains;

        @Nullable
        private Set<Class> allDomains;

        @Nullable
        private Set<Class> directRanges;

        @Nullable
        private Set<Class> allRanges;

        public Builder(@NotNull final String id, @Nullable final String label) {
            this.id = id;
            this.label = label;
        }

        @NotNull
        public Builder directDomains(@Nullable final Set<Class> directDomains) {
            this.directDomains = directDomains;
            return this;
        }

        @NotNull
        public Builder allDomains(@Nullable final Set<Class> allDomains) {
            this.allDomains = allDomains;
            return this;
        }

        @NotNull
        public Builder directRanges(@Nullable final Set<Class> directRanges) {
            this.directRanges = directRanges;
            return this;
        }

        @NotNull
        public Builder allRanges(@Nullable final Set<Class> allRanges) {
            this.allRanges = allRanges;
            return this;
        }

        @Override
        @NotNull
        public Relation build() {
            return new Relation(this);
        }
    }
}

