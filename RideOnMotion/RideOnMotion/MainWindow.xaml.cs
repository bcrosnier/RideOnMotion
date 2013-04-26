﻿using System.Windows;
using Microsoft.Kinect;
using KinectStatusNotifier;
using RideOnMotion.KinectModule;
using System;

namespace RideOnMotion
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// Active Kinect sensor controller.
        /// </summary>
        private KinectSensorController sensorController;

        /// <summary>
        /// Model view for this window.
        /// </summary>
        private MainWindowViewModel mainWindowViewModel;

		/// <summary>
		/// Kinect Status Notifier. Notably used by the Kinect system tray icon.
		/// </summary>
		private StatusNotifier notifier = new StatusNotifier();

		public MainWindow()
		{
            InitializeComponent();

            this.sensorController = new KinectSensorController();
            this.mainWindowViewModel = new MainWindowViewModel( sensorController );
            this.DataContext = this.mainWindowViewModel;
		}

		private void MainWindow_Loaded( object sender, RoutedEventArgs e )
		{
            prepareSensor();
		}

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.sensorController.StopSensor(); 
        }

        private void prepareSensor()
        {
            if ( !this.sensorController.HasSensor )
            {
                MessageBox.Show( "No Kinect device detected.\nPlease ensure it is plugged in and correctly installed.", "No Kinect detected" );
            }
            else
            {
                // Start Kinect
                this.sensorController.StartSensor(); // Blocking.
            }
        }

        /// <summary>
        /// File -> Quit action
        /// </summary>
        private void MenuItem_Quit_Click( object sender, RoutedEventArgs e )
        {
            this.Close();
        }
	}
}
