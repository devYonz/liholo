using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json.Linq;

namespace FaceTutorial
{
    public partial class MainWindow : Window
    {
        private readonly IFaceServiceClient faceServiceClient =
            new FaceServiceClient("d2f5dd1babbe48a8b8e0e6cc3bf39758", "https://westus.api.cognitive.microsoft.com/face/v1.0");

        string imagePath;
        string personGroupId = "testliholo_v1";
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

        private async void addFaceAndTrainData(string imagePath, System.Guid personId)
        {
            try
            {
                using (Stream imageFileStream = File.OpenRead(imagePath))
                {
                    AddPersistedFaceResult faceRes = await faceServiceClient.AddPersonFaceAsync(this.personGroupId, personId, imageFileStream);
                    Console.Out.WriteLine("Added face to Person with Id " + faceRes.PersistedFaceId);
                }


                await faceServiceClient.TrainPersonGroupAsync(this.personGroupId);
                TrainingStatus status = null;
                do
                {
                    status = await faceServiceClient.GetPersonGroupTrainingStatusAsync(this.personGroupId);
                } while (status.Status.ToString() != "Succeeded");
            }
            catch (FaceAPIException f)
            {
                MessageBox.Show(f.ErrorCode, f.ErrorMessage);
            }
        }
        private async void createPersonAndAddFace()
        {
            // Create a person
            System.Guid personId;
            try
            {
                string userData = Newtonsoft.Json.JsonConvert.SerializeObject(
                    new { Name = this.Name.Text });

                Console.Out.WriteLine("User Data is " + userData);
                CreatePersonResult res = await faceServiceClient.CreatePersonAsync(this.personGroupId, this.Name.Text, userData: userData);
                Console.Out.WriteLine("Created Peson with Id " + res.PersonId);
                personId = res.PersonId;
                
            }
            catch (FaceAPIException f)
            {
                MessageBox.Show(f.ErrorCode, f.ErrorMessage);
                return;
            }
        
            this.addFaceAndTrainData(imagePath, personId);
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
                string json_obj = Newtonsoft.Json.JsonConvert.SerializeObject(p.UserData);
                MessageBox.Show(
                    "Person Data is " + p.Name + " User data " + json_obj);
                Console.Out.WriteLine("User data: " + json_obj);
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
                        Console.Out.WriteLine("Match found for person " + res[i].Candidates[j].PersonId + " With confidence " + res[i].Candidates[j].Confidence);
                        // If the match is good for the person, then train the data with the current image.
                        if (res[i].Candidates[j].Confidence > 0.5)
                        {
                            this.addFaceAndTrainData(imagePath, res[i].Candidates[j].PersonId);
                        }
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