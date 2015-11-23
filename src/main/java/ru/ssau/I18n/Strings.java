package ru.ssau.I18n;

import java.util.ResourceBundle;

/**
 * Created by Sergei on 23.11.2015.
 */
public class Strings {

    //default locale
    private static ResourceBundle bundle;

    static{ bundle = ResourceBundle.getBundle("Strings"); }

    public static String getAppTitle() { return bundle.getString("AppTitle"); }

}
