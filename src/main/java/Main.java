package main.java;

import main.java.UI.MainShell;
import org.eclipse.swt.SWT;
import org.eclipse.swt.layout.GridLayout;
import org.eclipse.swt.widgets.Display;
import org.eclipse.swt.widgets.Label;
import org.eclipse.swt.widgets.Shell;

public class Main {

    private static void runmainshell(){
        Display display = new Display ();
        MainShell mainShell=new MainShell(display);
        display.dispose ();
    }
    public static void main (String [] args) {
        runmainshell();
    }
    private static void show(){
        Display display = new Display();
        Shell shell = new Shell(display);
        shell.setText("Label test");

        new Label(shell, SWT.SEPARATOR | SWT.HORIZONTAL);
        Label lblHello = new Label(shell, SWT.NONE);
        lblHello.setText("Simple Label");
        new Label(shell, SWT.SEPARATOR | SWT.HORIZONTAL);
        shell.setLayout(new GridLayout());
//        Label lblDuke = new Label(shell, SWT.NONE);
//        lblDuke.setImage(new Image(Display.getCurrent(), getClass()
//                .getResourceAsStream("/by/bs/swt/images/duke_waving.gif")));
        shell.pack();
        shell.open();
        while (!shell.isDisposed()) {
            if (!display.readAndDispatch())
                display.sleep();
        }
        shell.dispose();
        display.dispose();
    }
}
