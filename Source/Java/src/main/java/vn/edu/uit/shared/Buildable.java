package vn.edu.uit.shared;

import org.jetbrains.annotations.NotNull;

public interface Buildable<T> {
    @NotNull
    T build();
}
