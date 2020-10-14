using Matrox.MatroxImagingLibrary;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace MatroxHelp
{
    /// <summary>
    /// Interaction logic for the MIL Management view.
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private MIL_ID _app = MIL.M_NULL;
        private MIL_ID _system = MIL.M_NULL;
        private MIL_ID _buffer = MIL.M_NULL;
        private MIL_ID _digitizer = MIL.M_NULL;
        private MIL_ID _display = MIL.M_NULL;
        private MIL_ID _calibration = MIL.M_NULL;
        private MIL_ID _pvaCalibration = MIL.M_NULL;
        private MIL_ID _calibrationGraphics = MIL.M_NULL;
        private MIL_ID _calibrationGraphicsContext = MIL.M_NULL;

        private ICommand _toggleGrabCommand;
        private ICommand _closeCommand;
        private ICommand _calibrateFromImageCommand;
        private ICommand _calibrateFromDigitizerCommand;
        private ICommand _calibrateToPVACommand;

        private bool _canExecute = true;
        private bool _isGrabbing = false;
        private bool _isCalibrated = false;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        public MainWindowViewModel()
        {
            _toggleGrabCommand = new RelayCommand(ToggleGrab, param => _canExecute);
            _closeCommand = new RelayCommand(CloseApp, param => true);
            _calibrateFromImageCommand = new RelayCommand(CalibrateFromImage, param => true);
            _calibrateFromDigitizerCommand = new RelayCommand(CalibrateFromDigitizer, param => true);
            _calibrateToPVACommand = new RelayCommand(CalibrateToPVA, param => true);
            
            MIL.MappAllocDefault(MIL.M_DEFAULT, ref _app, ref _system, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL);

            MIL.MgraAlloc(_system, ref _calibrationGraphicsContext);
            MIL.MgraAllocList(_system, MIL.M_DEFAULT, ref _calibrationGraphics);

            // Allocate the standard dot grid calibration
            MIL.McalAlloc(_system, MIL.M_PERSPECTIVE_TRANSFORMATION, MIL.M_DEFAULT, ref _calibration);
            
            // Allocate the PVA calibration method using points from grid cal
            MIL.McalAlloc(_system, MIL.M_PERSPECTIVE_TRANSFORMATION, MIL.M_DEFAULT, ref _pvaCalibration);

            // Digitizer testing
            // Allocate digitizer
            MIL.MdigAlloc(_system, MIL.M_DEV0, "M_DEFAULT", MIL.M_DEV_NUMBER, ref _digitizer);
            MIL.MbufRestore(@".\CalImage.png", _system, ref _buffer);

            // 
            // // Inquire the _digitizer to determine the image buffer size.
            // MIL_INT bufferSizeX;
            // MIL_INT bufferSizeY;
            // MIL_INT bufferSizeBand;
            // 
            // bufferSizeX = MIL.MdigInquire(_digitizer, MIL.M_SIZE_X, MIL.M_NULL);
            // bufferSizeY = MIL.MdigInquire(_digitizer, MIL.M_SIZE_Y, MIL.M_NULL);
            // bufferSizeBand = MIL.MdigInquire(_digitizer, MIL.M_SIZE_BAND, MIL.M_NULL);
            // 
            // long imageAttributes = MIL.M_IMAGE | MIL.M_DISP | MIL.M_PROC | MIL.M_GRAB;
            // 
            // // Allocate the image buffer for digitizer
            // MIL.MbufAllocColor(_system, bufferSizeBand, bufferSizeX, bufferSizeY, 8 + MIL.M_UNSIGNED, imageAttributes, ref _buffer);

            // Setup Display
            MIL.MdispAlloc(_system, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_WPF, ref _display);
            MIL.MdispControl(_display, MIL.M_BACKGROUND_COLOR, MIL.M_COLOR_LIGHT_BLUE);
            MIL.MdispControl(_display, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE);
            MIL.MdispControl(_display, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE);
            MIL.MdispControl(_display, MIL.M_MOUSE_USE, MIL.M_ENABLE);

            // Associate display and buffer
            MIL.MdispSelect(_display, _buffer);
        }

        public MIL_ID DisplayID
        {
            get { return _display; }
            set
            {
                if (_display == value)
                    return;

                _display = value;
                PropertyChanged(this, new PropertyChangedEventArgs("DisplayID"));
            }
        }

        public bool IsGrabbing
        {
            get { return _isGrabbing; }
            set
            {
                if (_isGrabbing == value)
                    return;

                _isGrabbing = value;
                PropertyChanged(this, new PropertyChangedEventArgs("IsGrabbing"));
            }
        }
        public bool IsCalibrated
        {
            get { return _isCalibrated; }
            set
            {
                if (_isCalibrated == value)
                    return;

                _isCalibrated = value;
                PropertyChanged(this, new PropertyChangedEventArgs("IsCalibrated"));
            }
        }
        
        public ICommand ToggleGrabCommand
        {
            get
            {
                return _toggleGrabCommand;
            }
            set
            {
                _toggleGrabCommand = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ToggleGrabCommand"));
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                return _closeCommand;
            }
            set
            {
                _closeCommand = value;
                PropertyChanged(this, new PropertyChangedEventArgs("CloseCommand"));
            }
        }

        public ICommand CalibrateFromImageCommand
        {
            get
            {
                return _calibrateFromImageCommand;
            }
            set
            {
                _calibrateFromImageCommand = value;
                PropertyChanged(this, new PropertyChangedEventArgs("CalibrateFromImageCommand"));
            }
        }

        public ICommand CalibrateFromDigitizerCommand
        {
            get
            {
                return _calibrateFromDigitizerCommand;
            }
            set
            {
                _calibrateFromDigitizerCommand = value;
                PropertyChanged(this, new PropertyChangedEventArgs("CalibrateFromDigitizerCommand"));
            }
        }
        public ICommand CalibrateToPVACommand
        {
            get
            {
                return _calibrateToPVACommand;
            }
            set
            {
                _calibrateToPVACommand = value;
                PropertyChanged(this, new PropertyChangedEventArgs("CalibrateToPVACommand"));
            }
        }
        
        private void ToggleGrab(object obj)
        {
            if (IsGrabbing)
            {
                MIL.MdigHalt(_digitizer);
            }
            else
            {
                IsCalibrated = false;
                MIL.MdispControl(_display, MIL.M_ASSOCIATED_GRAPHIC_LIST_ID, MIL.M_NULL);
                MIL.MdigGrabContinuous(_digitizer, _buffer);
            }

            IsGrabbing = !IsGrabbing;

            // Associate display and buffer
            MIL.MdispSelect(_display, _buffer);
        }

        private void CloseApp(object obj)
        {
            if (IsGrabbing)
            {
                MIL.MdigHalt(_digitizer);
                IsGrabbing = false;
            }

            // Free MIL objects
            MIL.MdispFree(_display);

            MIL.MgraFree(_calibrationGraphicsContext);
            MIL.MgraFree(_calibrationGraphics);

            MIL.MbufFree(_buffer);

            // MIL.MdigFree(_digitizer);

            MIL.MsysFree(_system);

            MIL.MappFree(_app);
        }

        private void CalibrateFromImage(object obj)
        {
            MIL.MbufRestore(@".\CalImage.png", _system, ref _buffer);

            if (IsGrabbing)
            {
                ToggleGrab(null);
            }

            // Dot-Grid calibration
            try
            {
                MIL.McalGrid(_calibration, _buffer, 0, 0, 0, 16, 16, 2, 2, MIL.M_FULL_CALIBRATION, MIL.M_CIRCLE_GRID);

            }
            catch (MILException ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }

            MIL_INT result = MIL.M_NULL;
            MIL.McalInquire(_calibration, MIL.M_CALIBRATION_STATUS + MIL.M_TYPE_MIL_INT, ref result);
            if (result == MIL.M_CALIBRATED)
            {
                // Don't need to suppy graphics context to clear graphics
                MIL.MgraClear(MIL.M_DEFAULT, _calibrationGraphics);

                // Draw Pixel Coord Sys
                MIL.MgraControl(_calibrationGraphicsContext, MIL.M_COLOR, MIL.M_COLOR_RED);
                MIL.McalDraw(_calibrationGraphicsContext, MIL.M_NULL, _calibrationGraphics, MIL.M_DRAW_PIXEL_COORDINATE_SYSTEM + MIL.M_DRAW_AXES, MIL.M_DEFAULT, MIL.M_DEFAULT);

                // Draw World Coord Sys
                MIL.MgraControl(_calibrationGraphicsContext, MIL.M_COLOR, MIL.M_COLOR_GREEN);
                MIL.McalDraw(_calibrationGraphicsContext, _calibration, _calibrationGraphics, MIL.M_DRAW_ABSOLUTE_COORDINATE_SYSTEM + MIL.M_DRAW_AXES, MIL.M_DEFAULT, MIL.M_DEFAULT);

                // Move and draw Relative Coord Sys
                MIL.McalFixture(_calibration, MIL.M_NULL, MIL.M_MOVE_RELATIVE, MIL.M_POINT_AND_DIRECTION_POINT, MIL.M_DEFAULT, 15, 15, 20, 15);
                MIL.MgraControl(_calibrationGraphicsContext, MIL.M_COLOR, MIL.M_COLOR_BLUE);
                MIL.McalDraw(_calibrationGraphicsContext, _calibration, _calibrationGraphics, MIL.M_DRAW_RELATIVE_COORDINATE_SYSTEM + MIL.M_DRAW_AXES, MIL.M_DEFAULT, MIL.M_DEFAULT);

                // Associate calibration graphics to display
                MIL.MdispControl(_display, MIL.M_ASSOCIATED_GRAPHIC_LIST_ID, _calibrationGraphics);

                IsCalibrated = true;

                // Associate display and buffer
                MIL.MdispSelect(_display, _buffer);
            }
        }

        private void CalibrateFromDigitizer(object obj)
        {
            MIL.MbufClear(_buffer, MIL.M_COLOR_BLUE);
            if (IsGrabbing)
            {
                ToggleGrab(null);
            }

            MIL.MdigGrab(_digitizer, _buffer);

            try
            {
                MIL.McalGrid(_calibration, _buffer, 0, 0, 0, 16, 16, 2, 2, MIL.M_FULL_CALIBRATION, MIL.M_CIRCLE_GRID);

            }
            catch (MILException ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }

            MIL_INT result = MIL.M_NULL;
            MIL.McalInquire(_calibration, MIL.M_CALIBRATION_STATUS + MIL.M_TYPE_MIL_INT, ref result);
            if (result == MIL.M_CALIBRATED)
            {

                // Associate buffer with new calibration
                // MIL.McalAssociate(_calibration, _calibration, MIL.M_DEFAULT);

                // Don't need to suppy graphics context to clear graphics
                MIL.MgraClear(MIL.M_DEFAULT, _calibrationGraphics);

                // Draw Pixel Coord Sys
                MIL.MgraControl(_calibrationGraphicsContext, MIL.M_COLOR, MIL.M_COLOR_RED);
                MIL.McalDraw(_calibrationGraphicsContext, MIL.M_NULL, _calibrationGraphics, MIL.M_DRAW_PIXEL_COORDINATE_SYSTEM + MIL.M_DRAW_AXES, MIL.M_DEFAULT, MIL.M_DEFAULT);

                // Draw World Coord Sys
                MIL.MgraControl(_calibrationGraphicsContext, MIL.M_COLOR, MIL.M_COLOR_GREEN);
                MIL.McalDraw(_calibrationGraphicsContext, _calibration, _calibrationGraphics, MIL.M_DRAW_ABSOLUTE_COORDINATE_SYSTEM + MIL.M_DRAW_AXES, MIL.M_DEFAULT, MIL.M_DEFAULT);
                         
                MIL_INT angle = 42;
                // MIL.McalRelativeOrigin(_calibration, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL, 45, MIL.M_DEFAULT);
                // Move and draw Relative Coord Sys
                //MIL.McalFixture(_calibration, MIL.M_NULL, MIL.M_MOVE_RELATIVE, MIL.M_POINT_AND_DIRECTION_POINT, MIL.M_DEFAULT, 15, 15, 20, 15);

                MIL.MgraControl(_calibrationGraphicsContext, MIL.M_COLOR, MIL.M_COLOR_BLUE);
                MIL.McalDraw(_calibrationGraphicsContext, _calibration, _calibrationGraphics, MIL.M_DRAW_RELATIVE_COORDINATE_SYSTEM + MIL.M_DRAW_AXES, MIL.M_DEFAULT, MIL.M_DEFAULT);

                // Associate calibration graphics to display
                MIL.MdispControl(_display, MIL.M_ASSOCIATED_GRAPHIC_LIST_ID, _calibrationGraphics);

                // MIL_INT angle = MIL.M_NULL;
                //MIL.McalInquire(_calibration, MIL.M_RELATIVE_ORIGIN_ANGLE, ref angle);


                IsCalibrated = true;
            }

            // Associate display and buffer
            MIL.MdispSelect(_display, _buffer);
        }

        private void CalibrateToPVA(object obj)
        {
            // Inquire the digitizer to determine the image buffer size.
            MIL_INT bufferSizeX = MIL.MbufInquire(_buffer, MIL.M_SIZE_X, MIL.M_NULL);
            MIL_INT bufferSizeY = MIL.MbufInquire(_buffer, MIL.M_SIZE_Y, MIL.M_NULL);
            var refPointsX = new double[2];
            var refPointsY = new double[2];

            // Use 2 points of buffer for point and direction (X+ right, Y+ down). First should be at pixel center of buffer.
            refPointsX[0] = bufferSizeX / 2;
            refPointsX[1] = bufferSizeX * .75;
            refPointsY[0] = bufferSizeY / 2;
            refPointsY[1] = bufferSizeY / 2;

            // Get these 2 points in World units
            var horizontalPointsX = new double[2];
            var horizontalPointsY = new double[2];
            MIL.McalTransformCoordinateList(_calibration, MIL.M_PIXEL_TO_WORLD, 2, refPointsX, refPointsY, horizontalPointsX, horizontalPointsY);
            MIL.McalFixture(_calibration, MIL.M_NULL, MIL.M_MOVE_RELATIVE, MIL.M_POINT_AND_DIRECTION_POINT, _calibration, horizontalPointsX[0], horizontalPointsY[0], horizontalPointsX[1], horizontalPointsY[1]);

            // Get all calibration points, pixel and world
            var numPoints = 0;
            MIL.McalInquire(_calibration, MIL.M_NUMBER_OF_CALIBRATION_POINTS + MIL.M_TYPE_MIL_INT32, ref numPoints);

            var pixelPointsX = new double[numPoints];
            var pixelPointsY = new double[numPoints];

            var worldPointsX = new double[numPoints];
            var worldPointsY = new double[numPoints];
            var worldPointsZ = new double[numPoints];

            MIL.McalInquire(_calibration, MIL.M_CALIBRATION_IMAGE_POINTS_X, ref pixelPointsX[0]);
            MIL.McalInquire(_calibration, MIL.M_CALIBRATION_IMAGE_POINTS_Y, ref pixelPointsY[0]);
            MIL.McalInquire(_calibration, MIL.M_CALIBRATION_WORLD_POINTS_X, ref worldPointsX[0]);
            MIL.McalInquire(_calibration, MIL.M_CALIBRATION_WORLD_POINTS_Y, ref worldPointsY[0]);

            var alignedPointsX = new double[numPoints];
            var alignedPointsY = new double[numPoints];
            var alignedPointsZ = new double[numPoints];

            // TODO: How do I get the translated points like in DA when you call 
            // MIL.McalTransformCoordinateList(_calibration, _calibration, MIL.M_WORLD_TO_RELATIVE, 2, worldPointsX, worldPointsY, alignedPointsX, alignedPointsY);
            MIL.McalTransformCoordinate3dList(_calibration, MIL.M_ABSOLUTE_COORDINATE_SYSTEM, MIL.M_RELATIVE_COORDINATE_SYSTEM, numPoints, worldPointsX, worldPointsY, worldPointsZ, alignedPointsX, alignedPointsY, alignedPointsZ, MIL.M_DEFAULT);

            // Clear old graphics
            MIL.MgraClear(MIL.M_DEFAULT, _calibrationGraphics);
            MIL.McalList(_pvaCalibration, pixelPointsX, pixelPointsY, alignedPointsX, alignedPointsY, MIL.M_NULL, numPoints, MIL.M_FULL_CALIBRATION, MIL.M_DEFAULT);


            // Draw Pixel Coord Sys
            MIL.MgraControl(_calibrationGraphicsContext, MIL.M_COLOR, MIL.M_COLOR_RED);
            MIL.McalDraw(_calibrationGraphicsContext, MIL.M_NULL, _calibrationGraphics, MIL.M_DRAW_PIXEL_COORDINATE_SYSTEM + MIL.M_DRAW_AXES, MIL.M_DEFAULT, MIL.M_DEFAULT);

            // Draw World Coord Sys
            MIL.MgraControl(_calibrationGraphicsContext, MIL.M_COLOR, MIL.M_COLOR_GREEN);
            MIL.McalDraw(_calibrationGraphicsContext, _pvaCalibration, _calibrationGraphics, MIL.M_DRAW_ABSOLUTE_COORDINATE_SYSTEM + MIL.M_DRAW_AXES, MIL.M_DEFAULT, MIL.M_DEFAULT);

            // Move and draw Relative Coord Sys
            MIL.McalFixture(_pvaCalibration, MIL.M_NULL, MIL.M_MOVE_RELATIVE, MIL.M_POINT_AND_DIRECTION_POINT, MIL.M_DEFAULT, 15, 15, 20, 15);

            MIL.MgraControl(_calibrationGraphicsContext, MIL.M_COLOR, MIL.M_COLOR_BLUE);
            MIL.McalDraw(_calibrationGraphicsContext, _pvaCalibration, _calibrationGraphics, MIL.M_DRAW_RELATIVE_COORDINATE_SYSTEM + MIL.M_DRAW_AXES, MIL.M_DEFAULT, MIL.M_DEFAULT);

            // Associate final PVA calibration
            MIL.McalAssociate(_pvaCalibration, _buffer, MIL.M_DEFAULT);

            // Associate calibration graphics to display
            MIL.MdispControl(_display, MIL.M_ASSOCIATED_GRAPHIC_LIST_ID, _calibrationGraphics);
        }
    }
}
