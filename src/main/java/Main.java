import UI.MainShell;
import org.eclipse.swt.widgets.Display;
public class Main {

    public static void main (String [] args) {
        Display display = new Display ();
        MainShell mainShell=new MainShell(display);
        display.dispose ();
    }
}
