using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

namespace FaceTutorial
{
    public partial class MainWindow : Window
    {
        // Replace the first parameter with your valid subscription key.
        //
        // Replace or verify the region in the second parameter.
        //
        // You must use the same region in your REST API call as you used to obtain your subscription keys.
        // For example, if you obtained your subscription keys from the westus region, replace
        // "westcentralus" in the URI below with "westus".
        //
        // NOTE: Free trial subscription keys are generated in the westcentralus region, so if you are using
        // a free trial subscription key, you should not need to change this region.
        private readonly IFaceServiceClient faceServiceClient =
            new FaceServiceClient("d2f5dd1babbe48a8b8e0e6cc3bf39758", "https://westus.api.cognitive.microsoft.com/face/v1.0");

        Face[] faces;                   // The list of detected faces.
        String[] faceDescriptions;      // The list of descriptions for the detected faces.
        double resizeFactor;            // The resize factor for the displayed image.
        string imagePath;
        string personGroupId = "testlihalo_v2";
        string personGroupName = "testGroup";

        public MainWindow()
        {
            InitializeComponent();

        }

        // Displays the image and calls Detect Faces.
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the image file to scan from the user.
            var openDlg = new Microsoft.Win32.OpenFileDialog();

            openDlg.Filter = "JPEG Image(*.jpg)|*.jpg";
            bool? result = openDlg.ShowDialog(this);

            // Return if canceled.
            if (!(bool)result)
            {
                return;
            }

            // Display the image file.
            string filePath = openDlg.FileName;
            imagePath = filePath;
        }
  
        private async void createPersonGroup()
        {
            try
            {
                await faceServiceClient.CreatePersonGroupAsync(this.personGroupId, personGroupName);
            }
            catch (FaceAPIException f)
            {
                if (f.ErrorCode != "PersonGroupExists")
                {
                    MessageBox.Show(f.ErrorCode, f.ErrorMessage);
                    return;
                }
            }
        }

        private async void createPersonAndAddFace()
        {
            // Create a person
            try
            {
                CreatePersonResult res = await faceServiceClient.CreatePersonAsync(this.personGroupId, this.Profile_URL.Text);
                Console.Out.WriteLine("Created Peson with Id " + res.PersonId);
                System.Guid personId = res.PersonId;
                using (Stream imageFileStream = File.OpenRead(imagePath))
                {
                    AddPersistedFaceResult faceRes = await faceServiceClient.AddPersonFaceAsync(this.personGroupId, personId, imageFileStream);
                    Console.Out.WriteLine("Added face to Person with Id " + faceRes.PersistedFaceId);
                }
            }
            catch (FaceAPIException f)
            {
                MessageBox.Show(f.ErrorCode, f.ErrorMessage);
                return;
            }

            try
            {
                await faceServiceClient.TrainPersonGroupAsync(this.personGroupId);
                TrainingStatus status = null;
                do
                {
                   status = await faceServiceClient.GetPersonGroupTrainingStatusAsync(this.personGroupId);  
                } while (status.Status.ToString() != "Succeeded");
            }
            catch(FaceAPIException f)
            {
                MessageBox.Show(f.ErrorCode, f.ErrorMessage);
            }
        }

        private void storeInPersonGroup(object sender, RoutedEventArgs e)
        {
            this.createPersonGroup();
            this.createPersonAndAddFace();
        }

        private async void getPersonData(System.Guid id)
        {
            try
            {
                Person p = await faceServiceClient.GetPersonAsync(this.personGroupId, id);
                MessageBox.Show("Person Data is " + p.Name);
            } 
            catch (FaceAPIException f)
            {
                MessageBox.Show(f.ErrorCode, f.ErrorMessage);
                return;
            }  
        }
        private async void testWithFaceAPI(object sender, RoutedEventArgs e)
        {
            Face[] faces = null;
            try
            {
                Person[] pg = await faceServiceClient.ListPersonsAsync(this.personGroupId);
                Console.Out.WriteLine("Found " + pg.Length + " in group");
            } 
            catch(FaceAPIException f)
            {
                MessageBox.Show(f.ErrorCode, f.ErrorMessage);
            }

            using (Stream imageFileStream = File.OpenRead(imagePath))
            {
                faces = await faceServiceClient.DetectAsync(imageFileStream, returnFaceId: true);
            }

            System.Guid[] faceIds = new System.Guid[faces.Length];
            for (int i = 0; i < faces.Length; ++i)
            {
                faceIds[i] = faces[i].FaceId;
                Console.Out.WriteLine("Detected Face ID " + faces[i].FaceId);
            }

            try
            {
                IdentifyResult[] res = await faceServiceClient.IdentifyAsync(this.personGroupId, faceIds, maxNumOfCandidatesReturned: 1, confidenceThreshold: (float)0.3);
                for (int i=0; i < res.Length; ++i)
                {
                    Console.Out.WriteLine("Looking at face Id " + res[i].FaceId + " with Candidates " + res[i].Candidates.Length);
                    for (int j = 0; j < res[i].Candidates.Length; ++j)
                    {
                        Console.Out.WriteLine("Match found for person " + res[i].Candidates[j].PersonId);
                        this.getPersonData(res[i].Candidates[j].PersonId);
                    }
                }
            }
            catch (FaceAPIException f)
            {
                MessageBox.Show(f.ErrorCode, f.ErrorMessage);
            }
        }
    }
}