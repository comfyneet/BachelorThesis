package vn.edu.uit.ontologymanager.model;

import com.google.gson.annotations.SerializedName;
import org.jetbrains.annotations.NotNull;
import org.jetbrains.annotations.Nullable;
import vn.edu.uit.shared.Buildable;

import java.util.List;
import java.util.Set;

import static vn.edu.uit.ontologymanager.model.DataType.ENUMERATED;
import static vn.edu.uit.ontologymanager.model.EntityType.ATTRIBUTE;

public class Attribute extends Entity {

    @Nullable
    @SerializedName("DirectDomains")
    private final Set<Class> directDomains;

    @Nullable
    @SerializedName("AllDomains")
    private final Set<Class> allDomains;

    @Nullable
    @SerializedName("Range")
    private final DataType range;

    @Nullable
    @SerializedName("EnumeratedValues")
    private final List<String> enumeratedValues;

    Attribute(@NotNull final Builder builder) {
        super(ATTRIBUTE, builder.id, builder.label);
        directDomains = builder.directDomains;
        allDomains = builder.allDomains;
        range = builder.range;
        enumeratedValues = builder.enumeratedValues;
    }

    public static class Builder implements Buildable<Attribute> {

        @NotNull
        private final String id;

        @Nullable
        private final String label;

        @Nullable
        private Set<Class> directDomains;

        @Nullable
        private Set<Class> allDomains;

        @Nullable
        private DataType range;

        @Nullable
        private List<String> enumeratedValues;

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
        public Builder range(@Nullable final DataType range) {
            if (range != ENUMERATED) this.range = range;
            return this;
        }

        @NotNull
        public Builder enumeratedValues(@NotNull final List<String> enumeratedValues) {
            this.range = ENUMERATED;
            this.enumeratedValues = enumeratedValues;
            return this;
        }

        @Override
        @NotNull
        public Attribute build() {
            return new Attribute(this);
        }
    }
}
