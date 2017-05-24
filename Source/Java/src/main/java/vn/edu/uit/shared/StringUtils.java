package vn.edu.uit.shared;

import org.jetbrains.annotations.NotNull;

import java.text.Normalizer;
import java.util.regex.Pattern;

public class StringUtils {

    @NotNull
    public static String removeAccent(@NotNull final String s) {

        final String temp = Normalizer.normalize(s, Normalizer.Form.NFD);
        final Pattern pattern = Pattern.compile("\\p{InCombiningDiacriticalMarks}+");
        return pattern.matcher(temp).replaceAll("").replaceAll("Đ", "D").replace("đ", "d");
    }
}
