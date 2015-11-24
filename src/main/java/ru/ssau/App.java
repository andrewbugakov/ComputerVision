package ru.ssau;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.opencv.core.Core;
import org.opencv.core.Mat;
import org.opencv.imgcodecs.Imgcodecs;
import ru.ssau.i18n.Strings;

public class App 
{
	private static Logger logger = LogManager.getLogger(App.class.getName());

	static{ System.loadLibrary(Core.NATIVE_LIBRARY_NAME); }

	public static void main(String[] args) throws Exception {
		logger.debug("Application has started");

		String filePath = "src/main/resources/images/marble.jpg";
		
		Mat newImage = Imgcodecs.imread(filePath);

		if(newImage.dataAddr()==0){
			System.out.println("Couldn't open file " + filePath);
		}else{

			GUI gui = new GUI(Strings.getAppTitle(), newImage);
			gui.init();
		}
		return;
	}
}