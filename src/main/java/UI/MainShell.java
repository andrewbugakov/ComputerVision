package UI;

import org.eclipse.swt.graphics.Point;
import org.eclipse.swt.graphics.Rectangle;
import org.eclipse.swt.widgets.Display;
import org.eclipse.swt.widgets.Shell;

public class MainShell {
    public MainShell(Display display){
        Shell shell = new Shell(display);
        shell.setText("Point Cloud Generator preview");
        shell.setSize(500, 500);
        centerWindow(shell);
//        shell.setToolTipText("This is a window");
        shell.open();

        while (!shell.isDisposed()) {
            if (!display.readAndDispatch()) {
                display.sleep();
            }
        }
    }
    private void setWindowSize(Shell shell){
        Rectangle bds = shell.getDisplay().getBounds();
        shell.setSize(bds.width,bds.height);
    }
    private void centerWindow(Shell shell) {

        Rectangle bds = shell.getDisplay().getBounds();

        Point p = shell.getSize();

        int nLeft = (bds.width - p.x) / 2;
        int nTop = (bds.height - p.y) / 2;

        shell.setBounds(nLeft, nTop, p.x, p.y);
    }
}
