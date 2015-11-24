package ru.ssau.gui.core;

import javafx.util.Pair;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;

import javax.swing.*;
import java.awt.event.WindowAdapter;
import java.awt.event.WindowEvent;
import java.util.Stack;

/**
 * Created by Sergei on 24.11.2015.
 */
public class NavigationController {

    private static Logger logger = LogManager.getLogger(NavigationController.class.getSimpleName());

    private static Stack<Pair<Frameable, JFrame>> navigationStack = new Stack<Pair<Frameable, JFrame>>();

    public static void open(final Frameable view) {

        JFrame frame = view.getFrame();
        int closeOperation = JFrame.EXIT_ON_CLOSE;

        if (!navigationStack.empty()){
            closeOperation = JFrame.DO_NOTHING_ON_CLOSE;
            navigationStack.peek().getValue().setEnabled(false);
        }

        frame.addWindowListener(new WindowAdapter() {
            @Override
            public void windowClosing(WindowEvent e) {
                close(view);
            }
        });
        frame.setDefaultCloseOperation(closeOperation);
        frame.setVisible(true);
        navigationStack.push(new Pair<Frameable, JFrame>(view, frame));

        logger.debug("opened " + view.getClass().getName());
    }

    public static void close() {
        close(navigationStack.peek().getKey());
    }

    public static void close(final Frameable view) {

        int index = getIndexAtArray(view);
        assert index != -1 : "You try to close not opened file";

        JFrame frame = navigationStack.get(index).getValue();
        frame.setVisible(false);
        frame.dispose();

        navigationStack.remove(index);

        if (!navigationStack.empty()) {
            JFrame topFrame = navigationStack.peek().getValue();
            topFrame.setEnabled(true);
            topFrame.setAlwaysOnTop(true);
            topFrame.setAlwaysOnTop(false);
        }

        logger.debug("closed " + view.getClass().getName());
    }

    public static JFrame getEmptyFrame() {
        JFrame frame = new JFrame();
        frame.setSize(500, 500);
        return frame;
    }

    private static int getIndexAtArray(Frameable view) {
        for (int i = 0; i < navigationStack.size(); i++) {
            Pair<Frameable, JFrame> pair = navigationStack.get(i);
            if (pair.getKey() == view) {
                return i;
            }
        }
        return -1;
    }
}
