package vn.edu.uit.shared;

import com.google.gson.annotations.SerializedName;

public class Pair<L, R> {

    @SerializedName("Left")
    private final L left;

    @SerializedName("Right")
    private final R right;

    public Pair(final L left, final R right) {
        this.left = left;
        this.right = right;
    }

    public L getLeft() {
        return left;
    }

    public R getRight() {
        return right;
    }

    @Override
    public int hashCode() {
        return left.hashCode() ^ right.hashCode();
    }

    @Override
    public boolean equals(final Object o) {
        if (!(o instanceof Pair)) return false;
        final Pair pairO = (Pair) o;
        return this.left.equals(pairO.left) &&
                this.right.equals(pairO.right);
    }
}
