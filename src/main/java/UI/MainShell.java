package main.java.UI;

import org.eclipse.swt.SWT;
import org.eclipse.swt.events.SelectionAdapter;
import org.eclipse.swt.events.SelectionEvent;
import org.eclipse.swt.events.SelectionListener;
import org.eclipse.swt.graphics.Image;
import org.eclipse.swt.graphics.Point;
import org.eclipse.swt.graphics.Rectangle;
import org.eclipse.swt.layout.GridLayout;
import org.eclipse.swt.widgets.*;

import java.lang.reflect.Method;

public class MainShell implements Shellable {
    private Shell shell;
    private String[] data={"web-камера","файл с диска","загрузить видеозапись с youtube"};
    public MainShell(Display display) {
        init(display);
        run(display);
    }

    private void setWindowSize(Shell shell) {
        Rectangle bds = shell.getDisplay().getBounds();
        shell.setSize(bds.width, bds.height);
    }

    private void centerWindow(Shell shell) {

        Rectangle bds = shell.getDisplay().getBounds();

        Point p = shell.getSize();

        int nLeft = (bds.width - p.x) / 2;
        int nTop = (bds.height - p.y) / 2;

        shell.setBounds(nLeft, nTop, p.x, p.y);
    }

    protected Menu createMenu(Menu parent, boolean enabled) {
        Menu m = new Menu(parent);
        m.setEnabled(enabled);
        return m;
    }
    protected Menu createMenu(MenuItem parent, boolean enabled) {
        Menu m = new Menu(parent);
        m.setEnabled(enabled);
        return m;
    }
    protected Menu createMenu(Shell parent, int style) {
        Menu m = new Menu(parent, style);
        return m;
    }
    protected Menu createMenu(Shell parent, int style,
                              MenuItem container, boolean enabled) {
        Menu m = createMenu(parent, style);
        m.setEnabled(enabled);
        container.setMenu(m);
        return m;
    }
    protected Menu createPopupMenu(Shell shell) {
        Menu m = new Menu(shell, SWT.POP_UP);
        shell.setMenu(m);
        return m;
    }
    protected Menu createPopupMenu(Shell shell, Control owner) {
        Menu m = createPopupMenu(shell);
        owner.setMenu(m);
        return m;
    }
    protected MenuItem createMenuItem(Menu parent, int style, String text,
                                      Image icon, int accel, boolean enabled,
                                      String callback) {
        MenuItem mi = new MenuItem(parent, style);
        if (text != null) {
            mi.setText(text);
        }
        if (icon != null) {
            mi.setImage(icon);
        }
        if (accel != -1) {
            mi.setAccelerator(accel);
        }
        mi.setEnabled(enabled);
        if (callback != null) {
            registerCallback(mi, this, callback);
        }
        return mi;
    }

    protected void registerCallback(final MenuItem mi,
                                    final Object handler,
                                    final String handlerName) {
        mi.addSelectionListener(new SelectionAdapter() {
            public void widgetSelected(SelectionEvent e) {
                try {
                    Method m = handler.getClass().
                            getMethod(handlerName, null);
                    m.invoke(handler, null);
                }
                catch (Exception ex) {
                    ex.printStackTrace();
                }
            }
        });
    }

    private void menu(){
        Label body=new Label(shell,SWT.NONE);
        // Создание системы меню
        Menu main = createMenu(shell, SWT.BAR | SWT.LEFT_TO_RIGHT);
        shell.setMenuBar(main);

        MenuItem fileMenuItem = createMenuItem(main, SWT.CASCADE, "&File",
                null, -1, true, null);
        Menu fileMenu = createMenu(shell, SWT.DROP_DOWN, fileMenuItem, true);
        MenuItem exitMenuItem = createMenuItem(fileMenu, SWT.PUSH, "E&xit\tCtrl+X",
                null, SWT.CTRL + 'X', true, "doExit");

        MenuItem helpMenuItem = createMenuItem(main, SWT.CASCADE, "&Help",
                null, -1, true, null);
        Menu helpMenu = createMenu(shell, SWT.DROP_DOWN, helpMenuItem, true);
        MenuItem aboutMenuItem = createMenuItem(helpMenu, SWT.PUSH, "&About\tCtrl+A",
                null, SWT.CTRL + 'A', true, "doAbout");

// добавление всплывающего меню
        Menu popup = createPopupMenu(shell, body);
        MenuItem popupMenuItem1 = createMenuItem(popup, SWT.PUSH, "&About",
                null, -1, true, "doAbout");
        MenuItem popupMenuItem2 = createMenuItem(popup, SWT.PUSH, "&Noop",
                null, -1, true, "doNothing");

//        new Label(shell, SWT.SEPARATOR | SWT.HORIZONTAL).setAlignment(SWT.FILL);
    }
    @Override
    public void init(Display display) {
        shell = new Shell(display);
        shell.setSize(800, 500);
        GridLayout gridLayout=new GridLayout();
        gridLayout.numColumns = 3;
        shell.setText("Генератор облака точек видеозаписи");
        centerWindow(shell);
//        menu();
        Label helloLabel = new Label(shell, SWT.NONE);
        helloLabel.setText("Выберите источник видеозаписи: ");
        helloLabel.setLocation(0,0);

        final Button openFile=new Button(shell,SWT.NONE);
        openFile.setSize(50,50);
        openFile.setVisible(true);
        final Combo listVideoFrom=new Combo(shell,SWT.DROP_DOWN);
        listVideoFrom.addSelectionListener(new SelectionListener() {
            @Override
            public void widgetSelected(SelectionEvent selectionEvent) {
                MessageBox messageBox = new MessageBox(shell);
                messageBox.setMessage("Widget selected!! "+listVideoFrom.getText());
                messageBox.open();
                switch (listVideoFrom.getText()){
                    case "web-камера":
                        openFile.setText("web-камера");
                        openFile.pack();
                        break;
                    case "файл с диска":
                        openFile.setText("файл с диска");
                        break;
                    case "загрузить видеозапись с youtube":
                        openFile.setText("загрузить видеозапись с youtube");
                        break;
                    default:
                        break;
                }
            }

            @Override
            public void widgetDefaultSelected(SelectionEvent selectionEvent) {
                MessageBox messageBox = new MessageBox(shell);
                messageBox.setMessage("Выберите источник видеозаписей из списка!");
                messageBox.open();

            }
        });

        listVideoFrom.setItems(data);
        /*Button button=new Button(shell,SWT.FLAT);
        button.setText("!");
        button.pack();
        button.addSelectionListener(
                new org.eclipse.swt.events.SelectionAdapter() {
                    public void widgetSelected(
                            org.eclipse.swt.events.SelectionEvent e) {
                        MessageBox messageBox = new MessageBox(shell);
                        messageBox.setMessage("Ну,ахуеть теперь!");
                        messageBox.open();
                    }
                });*/





//        GridData gridData=new GridData();
//        gridData.horizontalAlignment=1;
//        gridData.horizontalAlignment = GridData.FILL;
//        helloLabel.setLayoutData(gridData);

        openFile.addSelectionListener(
                new org.eclipse.swt.events.SelectionAdapter() {
                    public void widgetSelected(
                            org.eclipse.swt.events.SelectionEvent e) {
                        MessageBox messageBox = new MessageBox(shell);
                        messageBox.setMessage("Ну,ахуеть теперь!");
                        messageBox.open();
                    }
                });
        shell.setLayout(gridLayout);
    }

    @Override
    public void run(Display display) {
        shell.open();

        while (!shell.isDisposed()) {
            if (!display.readAndDispatch()) {
                display.sleep();
            }
        }
    }
}
