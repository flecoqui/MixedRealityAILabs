using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Windows.System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI;

namespace PersonMaker
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        string authKey;
        string personGroupId;
        string personGroupName;

        Guid personId;
        string personName;
        StorageFolder personFolder;

        private FaceServiceClient faceServiceClient;
        private PersonGroup knownGroup;
        private int minPhotos = 6;

        public MainPage()
        {
            this.InitializeComponent();
            personName = string.Empty;
            authKey = string.Empty;
            personGroupId = string.Empty;
            personGroupName = string.Empty;
            personId = Guid.Empty;
        }

        /// <summary>
        /// Create a person group with ID and name provided if none can be found in the service.
        /// </summary>
        private async void CreatePersonGroupButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            personGroupId = PersonGroupIdTextBox.Text;
            personGroupName = PersonGroupNameTextBox.Text;
            PersonGroupStatusTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            authKey = AuthKeyTextBox.Text;
            faceServiceClient = new FaceServiceClient(authKey);

            if (null != faceServiceClient)
            {
                // You may experience issues with this below call, if you are attempting connection with
                // a service location other than 'West US'
                PersonGroup[] groups = await faceServiceClient.ListPersonGroupsAsync();
                var matchedGroups = groups.Where(p => p.PersonGroupId == personGroupId);

                if (matchedGroups.Count() > 0)
                {
                    knownGroup = matchedGroups.FirstOrDefault();

                    PersonGroupStatusTextBlock.Text = "Found existing: " + knownGroup.Name;
                }

                if (null == knownGroup)
                {
                    await faceServiceClient.CreatePersonGroupAsync(personGroupId, personGroupName);
                    knownGroup = await faceServiceClient.GetPersonGroupAsync(personGroupId);

                    PersonGroupStatusTextBlock.Text = "Created new group: " + knownGroup.Name;
                }

                if (PersonGroupStatusTextBlock.Text != "- Person Group status -")
                {
                    PersonGroupStatusTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    PersonGroupStatusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
        }

        private async void CreatePersonButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            personName = PersonNameTextBox.Text;
            PersonStatusTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            if (knownGroup != null && personName.Length > 0)
            {
                //Check if this person already exist
                bool personAlreadyExist = false;
                Person[] ppl = await GetKnownPeople();
                foreach (Person p in ppl)
                {
                    if (p.Name == personName)
                    {
                        personAlreadyExist = true;
                        PersonStatusTextBlock.Text = $"Person already exist: {p.Name} ID: {p.PersonId}";

                        PersonStatusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    }
                }

                if (!personAlreadyExist)
                {
                    CreatePersonResult result = await faceServiceClient.CreatePersonAsync(personGroupId, personName);
                    if (null != result && null != result.PersonId)
                    {
                        personId = result.PersonId;

                        PersonStatusTextBlock.Text = "Created new person: " + result.PersonId;

                        PersonStatusTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                    }
                }
            }
        }


        private async void CreateFolderButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (personName.Length > 0 && personId != Guid.Empty)
            {
                StorageFolder picturesFolder = KnownFolders.PicturesLibrary;
                personFolder = await picturesFolder.CreateFolderAsync(personName, CreationCollisionOption.OpenIfExists);
                await Launcher.LaunchFolderAsync(personFolder);
            }
        }



        private async void SubmitToAzureButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            int imageCounter = 0;
            var items = await personFolder.GetFilesAsync();
            List<StorageFile> imageFilesToUpload = new List<StorageFile>();
            foreach (StorageFile item in items)
            {
                //Windows Cam default save type is jpg
                if (item.FileType == ".jpg")
                {
                    imageCounter++;
                    imageFilesToUpload.Add(item);
                }
            }

            if (imageCounter >= minPhotos)
            {
                imageCounter = 0;
                try
                {
                    foreach (StorageFile imageFile in imageFilesToUpload)
                    {
                        imageCounter++;
                        using (Stream s = await imageFile.OpenStreamForReadAsync())
                        {
                            AddPersistedFaceResult addResult = await faceServiceClient.AddPersonFaceAsync(personGroupId, personId, s);
                            Debug.WriteLine("Add result: " + addResult + addResult.PersistedFaceId);

                        }
                        SubmissionStatusTextBlock.Text = "Submitted Image n. " + imageCounter;
                    }
                    SubmissionStatusTextBlock.Text = "Total Images submitted: " + imageCounter;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Submission Exc: " + ex.Message);
                }
            }
            else
            {
                SubmissionStatusTextBlock.Text = $"Please add at least {minPhotos} face images to the person folder.";
            }
        }

        private async void TrainButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (personGroupId.Length > 0)
            {
                await faceServiceClient.TrainPersonGroupAsync(personGroupId);

                TrainingStatus trainingStatus = null;
                while (true)
                {
                    trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId);

                    if (trainingStatus.Status != Status.Running)
                    {
                        break;
                    }
                    await Task.Delay(1000);
                }

                TrainStatusTextBlock.Text = "Training Completed!";
            }
        }

        private async void DeletePersonButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            personName = PersonNameTextBox.Text;
            bool personExist = false;
            Person[] ppl = await GetKnownPeople();
            foreach (Person p in ppl)
            {
                if (p.Name == personName)
                {
                    personExist = true;
                    PersonStatusTextBlock.Text = $"Deleting person: {p.Name} ID: {p.PersonId}";
                    await RemovePerson(p);
                }
            }
            if (!personExist)
            {
                PersonStatusTextBlock.Text = $"No persons found to delete.";
            }

        }

        internal async Task<Person[]> GetKnownPeople()
        {
            Person[] people = null;
            if (null != faceServiceClient)
            {
                people = await faceServiceClient.ListPersonsAsync(personGroupId);
            }
            return people;
        }

        internal async Task RemovePerson(Person person)
        {
            if (null != person)
            {
                await faceServiceClient.DeletePersonAsync(personGroupId, person.PersonId);
            }
        }
    }
}
