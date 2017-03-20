package vn.edu.uit.ontologymanager.model;

import com.google.gson.annotations.SerializedName;
import org.jetbrains.annotations.NotNull;
import org.jetbrains.annotations.Nullable;
import vn.edu.uit.shared.Buildable;

import java.util.Set;

import static vn.edu.uit.ontologymanager.model.EntityType.CLASS;

public class Class extends Entity {

    @Nullable
    @SerializedName("DirectSuperClasses")
    private final Set<Class> directSuperClasses;

    @Nullable
    @SerializedName("AllSuperClasses")
    private final Set<Class> allSuperClasses;

    @Nullable
    @SerializedName("DirectSubClasses")
    private final Set<Class> directSubClasses;

    @Nullable
    @SerializedName("AllSubClasses")
    private final Set<Class> allSubClasses;

    @Nullable
    @SerializedName("Attributes")
    private final Set<Attribute> attributes;

    @Nullable
    @SerializedName("DirectIndividuals")
    private final Set<Individual> directIndividuals;

    @Nullable
    @SerializedName("AllIndividuals")
    private final Set<Individual> allIndividuals;

    Class(@NotNull final Builder builder) {
        super(CLASS, builder.id, builder.label);
        directSuperClasses = builder.directSuperClasses;
        allSuperClasses = builder.allSuperClasses;
        directSubClasses = builder.directSubClasses;
        allSubClasses = builder.allSubClasses;
        attributes = builder.attributes;
        directIndividuals = builder.directIndividuals;
        allIndividuals = builder.allIndividuals;
    }

    public static class Builder implements Buildable<Class> {

        @NotNull
        private final String id;

        @Nullable
        private final String label;

        @Nullable
        private Set<Class> directSuperClasses;

        @Nullable
        private Set<Class> allSuperClasses;

        @Nullable
        private Set<Class> directSubClasses;

        @Nullable
        private Set<Class> allSubClasses;

        @Nullable
        private Set<Attribute> attributes;

        @Nullable
        private Set<Individual> directIndividuals;

        @Nullable
        private Set<Individual> allIndividuals;

        public Builder(@NotNull final String id, @Nullable final String label) {
            this.id = id;
            this.label = label;
        }

        @NotNull
        public Builder directSuperClasses(@Nullable final Set<Class> directSuperClasses) {
            this.directSuperClasses = directSuperClasses;
            return this;
        }

        @NotNull
        public Builder allSuperClasses(@Nullable final Set<Class> allSuperClasses) {
            this.allSuperClasses = allSuperClasses;
            return this;
        }

        @NotNull
        public Builder directSubClasses(@Nullable final Set<Class> directSubClasses) {
            this.directSubClasses = directSubClasses;
            return this;
        }

        @NotNull
        public Builder allSubClasses(@Nullable final Set<Class> allSubClasses) {
            this.allSubClasses = allSubClasses;
            return this;
        }

        @NotNull
        public Builder attributes(@Nullable final Set<Attribute> attributes) {
            this.attributes = attributes;
            return this;
        }

        @NotNull
        public Builder directIndividuals(@Nullable final Set<Individual> directIndividuals) {
            this.directIndividuals = directIndividuals;
            return this;
        }

        @NotNull
        public Builder allIndividuals(@Nullable final Set<Individual> allIndividuals) {
            this.allIndividuals = allIndividuals;
            return this;
        }

        @Override
        @NotNull
        public Class build() {
            return new Class(this);
        }
    }
}
