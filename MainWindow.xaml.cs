using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Frozen;
using System.IO.Enumeration;
using System.Security.Cryptography.X509Certificates;
using System.DirectoryServices.ActiveDirectory;

namespace Senior_Project___Texture_Pack_Maker
{
    /// <summary>
    /// Very weird coding practices that I intend to improve upon at a later date. A lot of the coding
    /// in this project is done under the time pressure of a Senior Capstone class whilst taking 15 other credits.
    /// I intend to neaten up this code as well as update it to become a very useful tool for the Geometry Dash Community.
    /// 
    /// I also will continue to publish all versions of the code on github making it completely open source, feel free
    /// to leave me feedback about my bad code practices so I can hopefully improve this tool for all of us, as well as continue
    /// to grow as a developer.
    /// </summary>

    public partial class MainWindow : Window
    {
        public string gProjectLocation = "";
        public Hashtable gMenuChanges = new Hashtable();
        int offset_x = 0;
        int offset_y = 0;
        int source_x = 0;
        int source_y = 0;
        string current_hovered_image = "";
        public MainWindow()
        {
            InitializeComponent();

            this.SizeChanged += MainWindow_SizeChanged;
            canvas.SizeChanged += Canvas_SizeChanged;
            this.KeyDown += Window_KeyDown;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            canvas.Width = WorkSpace.Width;
            canvas.Height = WorkSpace.Height;
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (UIElement child in canvas.Children)
            {
                if (child is Image)
                {
                    Image image = (Image)child;
                    image.Width = canvas.ActualWidth;
                    image.Height = canvas.ActualHeight;
                }
            }
        }
        private void Clear_Project()
        {
            // Helper function to be implemented at later time. When a user creates a project
            // the existing project should be closed out so that new work can be done. Likewise
            // if the project is closed somehow.
        }
        private void New_Project(object sender, RoutedEventArgs e)
        {
            NewProject newProject = new NewProject();
            newProject.ShowDialog();
            if (newProject.quit)
            {
                return;
            }
            gProjectLocation = newProject.project;

            string projectPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!Directory.Exists(projectPath + @"\LakeTexturePackMaker"))
            {
                Directory.CreateDirectory(projectPath + @"\LakeTexturePackMaker");
            }
                projectPath = projectPath + @"\LakeTexturePackMaker\" + gProjectLocation; 
            if (Directory.Exists(projectPath))
            {
                MessageBox.Show(gProjectLocation + " project already exists.", "Project Creation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            } else
            {
                Directory.CreateDirectory(projectPath);

                gProjectLocation = projectPath;

                string confirmationText = "Project created.";
                string caption = "Texture Pack Maker";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxResult result;

                result = MessageBox.Show(confirmationText, caption, button);
                Unset_Workspace(sender, e);
            }
        }

        private void Open_Project(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog ProjectFolder = new OpenFolderDialog();
            ProjectFolder.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\LakeTexturePackMaker";

            ProjectFolder.Title = "Choose a location to save new texture pack project.";

            if (ProjectFolder.ShowDialog() == false)
            {
                System.Exception exception = new System.Exception("No folder provided for image dump.");
                return;
            }

            gProjectLocation = ProjectFolder.FolderName;
            Clear_Project();
            Unset_Workspace(sender, e);
        }
        private void Open_Project_Location(object sender, RoutedEventArgs e)
        {
            if (gProjectLocation == "")
            {
                MessageBox.Show("Unable to open a project when none are open. First create a project by going to `File > New Project`.", "Open Project Location Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Process.Start("explorer.exe", gProjectLocation);
        }

            private void Split(object sender, RoutedEventArgs e)
        {
            // Fetch image for splitting.
            if (gProjectLocation == "")
            {
                MessageBox.Show("Unable to split gamesheets. First create a project by going to `File > New Project`.", "Add Gamesheet Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OpenFileDialog splitImageDialog = new OpenFileDialog();

            splitImageDialog.Filter = "PNG file (*.png)|*.png";
            splitImageDialog.Title = "Select a gamesheet for splitting.";

            BitmapImage image = new BitmapImage();
            if (splitImageDialog.ShowDialog() == false)
            {
                System.Exception exception = new System.Exception("No image provided for texture splitting.");
                return;
            }
            image.BeginInit();
            image.UriSource = new Uri(splitImageDialog.FileName);
            image.EndInit();

            // Fetch plist for splitting.

            OpenFileDialog splitPlistDialog = new OpenFileDialog();

            splitPlistDialog.Filter = "PLIST file (*.plist)|*.plist";
            splitPlistDialog.Title = "Select the corresponding plist file.";

            // System.IO.FileStream plist;
            if (splitPlistDialog.ShowDialog() == false)
            {
                System.Exception exception = new System.Exception("No plist provided for texture splitting.");
                return;
            }

            StreamReader plist = null;
            try
            {
                plist = new StreamReader(splitPlistDialog.FileName.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            // Reading the plist and outputting image locations

            Hashtable frames = new Hashtable();
            string FileName = "";

            using (plist)
            {
                byte[] b = new byte[8192];
                UTF8Encoding temp = new UTF8Encoding(true);
                int readLen;
                bool CurrentlyParsing = false;

                string ImageName = "";
                string[] ImageData = new string[] { };

                string line = plist.ReadLine();
                while (line != null)
                {
                    var parts = line.Split('\n');
                    foreach (var part in parts)
                    {
                        if (part.Contains(".png") && part.Contains("<key>"))
                        {
                            ImageName = part.Trim();
                            var nameParts = ImageName.Split('<', '>');
                            ImageName = nameParts[2];
                            CurrentlyParsing = true;
                        }
                        if (CurrentlyParsing && part.Contains("<string>"))
                        {
                            var entries = part.Split('{', '}', ',');
                            if (entries[1] != "")
                                ImageData = ImageData.Append<string>(entries[1]).ToArray();
                            ImageData = ImageData.Append<string>(entries[2]).ToArray();
                            if (entries[1] == "")
                            {
                                ImageData = ImageData.Append<string>(entries[3]).ToArray();
                                ImageData = ImageData.Append<string>(entries[6]).ToArray();
                                ImageData = ImageData.Append<string>(entries[7]).ToArray();
                            }

                        }
                        if (CurrentlyParsing && part.Contains("<false/>"))
                        {
                            ImageData = ImageData.Append<string>("0").ToArray();
                        }
                        else if (CurrentlyParsing && part.Contains("<true/>"))
                        {
                            ImageData = ImageData.Append<string>("1").ToArray();
                        }
                        if (CurrentlyParsing && part.Contains("</dict>"))
                        {
                            CurrentlyParsing = false;
                            frames.Add(ImageName, ImageData);

                            ImageName = "";
                            ImageData = new string[] { };
                        }
                        if (part.Contains(".png") && part.Contains("<string>"))
                        {
                            FileName = part.Trim();
                            var nameParts = FileName.Split('<', '>', '.');
                            FileName = nameParts[2];

                        }
                    }
                    line = plist.ReadLine();
                }
            }

            BitmapImage rotated_image = new BitmapImage();
            rotated_image.BeginInit();
            rotated_image.UriSource = new Uri(splitImageDialog.FileName);
            rotated_image.Rotation = Rotation.Rotate270;
            rotated_image.EndInit();

            if (!Directory.Exists(gProjectLocation + @"\" + FileName))
            {
                Directory.CreateDirectory(gProjectLocation + @"\" + FileName);
            }

            foreach (DictionaryEntry de in frames)
            {
                if (de.Value is string[] && de.Value is not null)
                {
                    var strings = de.Value as string[];
                    float x = float.Parse(strings[6]);
                    float y = float.Parse(strings[7]);
                    float width = float.Parse(strings[8]);
                    float height = float.Parse(strings[9]);
                    float rotated = float.Parse(strings[10]);

                    if (rotated == 1)
                    {
                        float temp = width; width = height; height = temp;
                        temp = x; x = y;
                        y = rotated_image.PixelWidth - temp - width;
                    }

                    Image newImage = new Image();
                    if (rotated == 1)
                        newImage.Source = rotated_image;
                    else
                        newImage.Source = image;

                    Image croppedImage = new Image();
                    croppedImage.Width = width;
                    croppedImage.Height = height;

                    CroppedBitmap cb;
                    if (rotated == 1)
                    {
                        cb = new CroppedBitmap(
                            (BitmapSource)rotated_image,
                            new Int32Rect((int)MathF.Floor(x), (int)MathF.Floor(y), (int)(height), (int)(width)));
                        croppedImage.Source = cb;
                    }
                    else
                    {
                        cb = new CroppedBitmap(
                            (BitmapSource)image,
                            new Int32Rect((int)MathF.Floor(x), (int)MathF.Floor(y), (int)(width), (int)(height)));
                        croppedImage.Source = cb;
                    }

                    RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)MathF.Ceiling(cb.PixelWidth), (int)MathF.Ceiling(cb.PixelHeight), cb.DpiX, cb.DpiY, PixelFormats.Default);

                    DrawingVisual dv = new DrawingVisual();
                    using (DrawingContext context = dv.RenderOpen())
                    {
                        context.DrawImage(cb, new Rect(0, 0, croppedImage.Source.Width, croppedImage.Source.Height));
                        context.Close();
                    }

                    renderTargetBitmap.Render(dv);
                    PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
                    pngBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                    string filepath = gProjectLocation + @"\" + FileName + @"\" + de.Key.ToString();

                    using (FileStream fs = new FileStream(filepath, FileMode.Create))
                    {
                        pngBitmapEncoder.Save(fs);
                        fs.Flush();
                        fs.Close();
                    }

                }

            }

            using (StreamWriter sw = new StreamWriter(gProjectLocation + @"\" + FileName + ".plist"))
            {
                string line = "";
                using (StreamReader sr = new StreamReader(splitPlistDialog.FileName.ToString()))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        sw.WriteLine(line);
                    }
                }
            }
            
            string confirmationText = "Successfully split gamesheet!";
            string caption = "Texture Pack Maker";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxResult result;

            result = MessageBox.Show(confirmationText, caption, button);

        }

        // TODO:
        // Modify the merge to look for every directory in the project directory and merging with the plist with the same name as the folder.
        // Doing so should replace the plist in the project directory as well as generate the new one in the users selected location.
        private void Merge(object sender, RoutedEventArgs e)
        {
            if (gProjectLocation == "")
            {
                MessageBox.Show("You cannot merge a nonexistant project. To create a project go to `File > New Project`.", "Generate Texture Pack Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (gMenuChanges.Count > 0)
            {
                Save(sender, e);
            }

            OpenFolderDialog dump = new OpenFolderDialog();
            dump.Title = "Select a directory to save the generated texture files.";
            if (dump.ShowDialog() == false)
            {
                System.Exception exception = new System.Exception("No folder selected before quitting.");
                return;
            }

            var gameSheets = Directory.EnumerateDirectories(gProjectLocation);
            foreach (var gamesheet in gameSheets)
            {
                Hashtable frames = new Hashtable();
                var plistParts = gamesheet.Split('\\');
                var plistName = plistParts[plistParts.Length - 1];
                StreamReader plist = new StreamReader(gProjectLocation + @"/" + plistName + ".plist");
                String[] keys = new string[] { };
                BitmapImage[] bmis = new BitmapImage[] { };

                string FileName = "";
                using (plist)
                {
                    // plist.FlushAsync();
                    byte[] b = new byte[8192];
                    UTF8Encoding temp = new UTF8Encoding(true);
                    int readLen;
                    bool CurrentlyParsing = false;

                    string ImageName = "";
                    string[] ImageData = new string[] { };

                    string line = plist.ReadLine();
                    while (line != null)
                    {
                        var parts = line.Split('\n');
                        foreach (var part in parts)
                        {
                            if (part.Contains(".png") && part.Contains("<key>"))
                            {
                                ImageName = part.Trim();
                                var nameParts = ImageName.Split('<', '>');
                                ImageName = nameParts[2];

                                var imgs = Directory.EnumerateFiles(gamesheet, ImageName);
                                Debug.Assert(imgs != null);
                                foreach (var img in imgs)
                                {
                                    BitmapImage bmi = new BitmapImage();
                                    bmi.BeginInit();
                                    bmi.UriSource = new Uri(img);
                                    bmi.EndInit();
                                    bmis = bmis.Append<BitmapImage>(bmi).ToArray();

                                    var segments = img.Split('\\');
                                    keys = keys.Append<String>(segments[segments.Length - 1]).ToArray();

                                }

                                CurrentlyParsing = true;
                            }
                            if (CurrentlyParsing && part.Contains("<string>"))
                            {
                                var entries = part.Split('{', '}', ',');
                                if (entries[1] != "")
                                    ImageData = ImageData.Append<string>(entries[1]).ToArray();
                                ImageData = ImageData.Append<string>(entries[2]).ToArray();
                                if (entries[1] == "")
                                {
                                    ImageData = ImageData.Append<string>(entries[3]).ToArray();
                                    ImageData = ImageData.Append<string>(entries[6]).ToArray();
                                    ImageData = ImageData.Append<string>(entries[7]).ToArray();
                                }

                            }
                            if (CurrentlyParsing && part.Contains("<false/>"))
                            {
                                ImageData = ImageData.Append<string>("0").ToArray();
                            }
                            else if (CurrentlyParsing && part.Contains("<true/>"))
                            {
                                ImageData = ImageData.Append<string>("1").ToArray();
                            }
                            if (CurrentlyParsing && part.Contains("</dict>"))
                            {
                                CurrentlyParsing = false;
                                frames.Add(ImageName, ImageData);

                                ImageName = "";
                                ImageData = new string[] { };
                            }
                            if (part.Contains(".png") && part.Contains("<string>"))
                            {
                                FileName = part.Trim();
                                var nameParts = FileName.Split('<', '>', '.');
                                FileName = nameParts[2];

                            }
                        }
                        line = plist.ReadLine();
                    }
                }

                //
                // The above code was copy pasted from splitting, at a later time create a helper function and call it in both methods.
                //

                // Implement sorting algorithm to sort bmis by size to compact gamesheet size.
                // Or find a way to organize the DrawingContext in an optimal way. Try and avoid allowing rotations. We aren't going for
                //   super compact, just enough to where it doesn't feel likewasting space.

                DrawingVisual dv = new DrawingVisual();

                double padding_constant = 2;
                int images_per_row = 20;

                double x_offset = 0;
                double y_offset = 0;
                double total_bmis = 0;
                double largest_height = 0;
                double largest_width = 0;
                int images_row_counter = 0;

                Hashtable new_image_data = new Hashtable();

                using (DrawingContext context = dv.RenderOpen())
                {
                    foreach (var bmi in bmis)
                    {
                        context.DrawImage(bmi, new Rect(0 + x_offset, 0 + y_offset, bmi.PixelWidth, bmi.PixelHeight));

                        string[] image_data = { x_offset.ToString(), y_offset.ToString(), bmi.PixelWidth.ToString(), bmi.PixelHeight.ToString() };
                        new_image_data.Add(keys[(int)total_bmis], image_data);
                        // This ONLY works so long as the keys list and png list are in the same order.

                        x_offset += bmi.PixelWidth + padding_constant;
                        total_bmis++;
                        if (bmi.PixelHeight > largest_height)
                            largest_height = bmi.PixelHeight;
                        images_row_counter++;
                        if (images_row_counter >= images_per_row)
                        {
                            y_offset += largest_height + padding_constant;
                            largest_height = 0;
                            if (x_offset > largest_width)
                                largest_width = x_offset;
                            x_offset = 0;
                            images_row_counter = 0;
                        }
                    }
                    if (largest_width == 0)
                        largest_width = x_offset;
                    y_offset += largest_height;
                    context.Close();
                }

                double image_width = largest_width;
                double image_height = y_offset;

                RenderTargetBitmap rtb = new RenderTargetBitmap((int)image_width, (int)image_height, 96, 96, PixelFormats.Default);

                rtb.Render(dv);
                PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
                pngBitmapEncoder.Frames.Add(BitmapFrame.Create(rtb));

                string png_filepath = dump.FolderName + @"\" + FileName + ".png";

                using (FileStream fs = new FileStream(png_filepath, FileMode.Create))
                {
                    pngBitmapEncoder.Save(fs);
                    fs.Flush();
                    fs.Close();
                }

                string plist_filepath = dump.FolderName + @"\" + FileName + ".plist";
                StreamReader plist2 = new StreamReader(gProjectLocation + @"/" + plistName + ".plist");

                string[] lines = new string[] { };
                for (int i = 0; i < 6; i++)
                {
                    lines = lines.Append<string>(plist2.ReadLine()).ToArray();
                }
                foreach (var key in keys)
                {
                    string[] image_data = new_image_data[key] as string[];
                    string[] f = frames[key] as string[];
                    int[] extraData = { 0, 0, 0, 0 };
                    var sox = float.Parse(f[0]);
                    var soy = float.Parse(f[1]);
                    var x = float.Parse(image_data[0]);
                    var y = float.Parse(image_data[1]);
                    var width = float.Parse(image_data[2]);
                    var height = float.Parse(image_data[3]);
                    var source_width = float.Parse(f[4]);
                    var source_height = float.Parse(f[5]);

                    lines = lines.Append<string>("\t\t\t<key>" + key + "</key>").ToArray();
                    lines = lines.Append<string>("\t\t\t<dict>").ToArray();
                    lines = lines.Append<string>("\t\t\t\t<key>aliases</key>").ToArray();
                    lines = lines.Append<string>("\t\t\t\t<array/>").ToArray();
                    lines = lines.Append<string>("\t\t\t\t<key>spriteOffset</key>").ToArray();
                    lines = lines.Append<string>("\t\t\t\t<string>{" + (int)sox + "," + (int)soy + "}</string>").ToArray();
                    lines = lines.Append<string>("\t\t\t\t<key>spriteSize</key>").ToArray();
                    lines = lines.Append<string>("\t\t\t\t<string>{" + (int)width + "," + (int)height + "}</string>").ToArray();
                    lines = lines.Append<string>("\t\t\t\t<key>spriteSourceSize</key>").ToArray();
                    lines = lines.Append<string>("\t\t\t\t<string>{" + (int)source_width + "," + (int)source_height + "}</string>").ToArray();
                    lines = lines.Append<string>("\t\t\t\t<key>textureRect</key>").ToArray();
                    lines = lines.Append<string>("\t\t\t\t<string>{{" + (int)x + "," + (int)y + "},{" + (int)width + "," + (int)height + "}}</string>").ToArray();
                    lines = lines.Append<string>("\t\t\t\t<key>textureRotated</key>").ToArray();
                    lines = lines.Append<string>("\t\t\t\t<false/>").ToArray();
                    lines = lines.Append<string>("\t\t\t</dict>").ToArray();
                }

                string output_file_name = FileName + ".png";

                lines = lines.Append<string>("\t\t</dict>").ToArray();
                lines = lines.Append<string>("\t\t<key>metadata</key>").ToArray();
                lines = lines.Append<string>("\t\t<dict>").ToArray();
                lines = lines.Append<string>("\t\t\t<key>format</key>").ToArray();
                lines = lines.Append<string>("\t\t\t<integer>3</integer>").ToArray();
                lines = lines.Append<string>("\t\t\t<key>pixelFormat</key>").ToArray();
                lines = lines.Append<string>("\t\t\t<string>RGBA8888</string>").ToArray();
                lines = lines.Append<string>("\t\t\t<key>premultiplyAlpha</key>").ToArray();
                lines = lines.Append<string>("\t\t\t<false/>").ToArray();
                lines = lines.Append<string>("\t\t\t<key>realTextureFileName</key>").ToArray();
                lines = lines.Append<string>("\t\t\t<string>" + output_file_name + "</string>").ToArray();
                lines = lines.Append<string>("\t\t\t<key>size</key>").ToArray();
                lines = lines.Append<string>("\t\t\t<string>{" + ((int)image_width).ToString() + "," + ((int)image_height).ToString() + "}</string>").ToArray();
                lines = lines.Append<string>("\t\t\t<key>smartupdate</key>").ToArray();
                lines = lines.Append<string>("\t\t\t<string>$TexturePacker:SmartUpdate:5d991255e6bb25e5cf649a1760178913:3e387e12e5a509b41d6606885194d713:cd3ab045ff8ec89044c566bf3c4878b7$</string>").ToArray();
                lines = lines.Append<string>("\t\t\t<key>textureFileName</key>").ToArray();
                lines = lines.Append<string>("\t\t\t<string>" + output_file_name + "</string>").ToArray();
                lines = lines.Append<string>("\t\t</dict>").ToArray();
                lines = lines.Append<string>("\t</dict>").ToArray();
                lines = lines.Append<string>("</plist>").ToArray();

                using (StreamWriter outputFile = new StreamWriter(plist_filepath))
                {
                    foreach (string line in lines)
                    {
                        outputFile.WriteLine(line);
                    }
                    outputFile.Close();
                }
                plist2.Close();
                using (StreamWriter outputFile = new StreamWriter(gProjectLocation + @"/" + plistName + ".plist"))
                {
                    foreach (string line in lines)
                    {
                        outputFile.WriteLine(line);
                    }
                    outputFile.Close();
                }
            }

            string confirmationText = "Successfully merged all gamesheets!";
            string caption = "Texture Pack Maker";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxResult result;

            result = MessageBox.Show(confirmationText, caption, button);
            gMenuChanges.Clear();
        }
        public Hashtable read_plist(StreamReader plist)
        {
            Hashtable plist_dict = new Hashtable();
            using (plist)
            {
                byte[] b = new byte[8192];
                UTF8Encoding temp = new UTF8Encoding(true);
                bool CurrentlyParsing = false;

                string ImageName = "";
                string[] ImageData = new string[] { };

                string line = plist.ReadLine();
                while (line != null)
                {
                    var parts = line.Split('\n');
                    foreach (var part in parts)
                    {
                        if (part.Contains(".png") && part.Contains("<key>"))
                        {
                            ImageName = part.Trim();
                            var nameParts = ImageName.Split('<', '>');
                            ImageName = nameParts[2];
                            CurrentlyParsing = true;
                        }
                        if (CurrentlyParsing && part.Contains("<string>"))
                        {
                            var entries = part.Split('{', '}', ',');
                            if (entries[1] != "")
                                ImageData = ImageData.Append<string>(entries[1]).ToArray();
                            ImageData = ImageData.Append<string>(entries[2]).ToArray();
                            if (entries[1] == "")
                            {
                                ImageData = ImageData.Append<string>(entries[3]).ToArray();
                                ImageData = ImageData.Append<string>(entries[6]).ToArray();
                                ImageData = ImageData.Append<string>(entries[7]).ToArray();
                            }

                        }
                        if (CurrentlyParsing && part.Contains("<false/>"))
                        {
                            ImageData = ImageData.Append<string>("0").ToArray();
                        }
                        else if (CurrentlyParsing && part.Contains("<true/>"))
                        {
                            ImageData = ImageData.Append<string>("1").ToArray();
                        }
                        if (CurrentlyParsing && part.Contains("</dict>"))
                        {
                            CurrentlyParsing = false;
                            plist_dict.Add(ImageName, ImageData);

                            ImageName = "";
                            ImageData = new string[] { };
                        }
                    }
                    line = plist.ReadLine();
                }
            }

            return plist_dict;
        }
        public void Save(object sender, RoutedEventArgs e)
        {
            if (gMenuChanges.Count == 0)
            {
                return;
            }
            foreach (DictionaryEntry de in gMenuChanges)
            {
                if (de.Value is int[] && de.Value is not null)
                {
                    var data = de.Value as int[];
                    var gameSheets = Directory.EnumerateFiles(gProjectLocation);
                    foreach (var gamesheet in gameSheets)
                    {
                        string[] lines = File.ReadAllLines(gamesheet);

                        for (int i = 0; i < lines.Length; i++)
                        {
                            if (lines[i].Contains("<key>" + de.Key.ToString() + ".png</key>"))
                            {
                                var line_data = lines[i + 5].Split('{', ',', '}');
                                lines[i + 5] = "\t\t\t\t<string>{" + (int.Parse(line_data[1]) + data[0]).ToString() + ", " + (int.Parse(line_data[2]) + data[1]).ToString() + "}</string>";

                                line_data = lines[i + 7].Split('{', ',', '}');
                                lines[i + 7] = "\t\t\t\t<string>{" + (int.Parse(line_data[1]) + data[2]).ToString() + ", " + (int.Parse(line_data[2]) + data[3]).ToString() + "}</string>";
                            }
                        }

                        File.WriteAllLines(gamesheet, lines);
                    }
                }
            }
            gMenuChanges.Clear();
        }
        public void add_image_to_canvas(ref Canvas canvas, string imageName, string fileLocation, Hashtable frames, bool gameSheet_exists, int left, int top, int menuOffset)
        {
            Image image = new Image();
            BitmapImage bmi = new BitmapImage();
            bmi.BeginInit();
            if (gameSheet_exists)
                bmi.UriSource = new Uri(gProjectLocation + @"/" + fileLocation + @"/" + imageName, UriKind.RelativeOrAbsolute);
            else
                bmi.UriSource = new Uri(@"/WorkspaceImages/MissingTexture.png", UriKind.RelativeOrAbsolute);
            bmi.EndInit();
            image.Source = bmi;
            image.Name = imageName.Split('.')[0];
            image.MouseDown += Image_MouseDown;
            image.Stretch = Stretch.Fill;

            float left_offset = 0;
            float top_offset = 0;
            if (gameSheet_exists) {
                string[] data = frames[imageName] as string[];
                image.Width = float.Parse(data[2]) * .5;
                image.Height = float.Parse(data[3]) * .5;
                left_offset = float.Parse(data[0]);
                top_offset = float.Parse(data[1]);
            }

            canvas.Children.Add(image);
            Canvas.SetLeft(image, ((left) - image.Width / 2) + left_offset);
            Canvas.SetTop(image, ((top + menuOffset) - image.Height / 2) + top_offset);
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (current_hovered_image == "")
            {
                return;
            }
            switch (e.Key)
            {
                case Key.Up:
                    // Offset += 1, SetTop -= 1 (GD and WPF have different y axis.
                    offset_y += 1;
                    foreach (UIElement child in  canvas.Children)
                    {
                        if (child is Image)
                        {
                            Image image = (Image)child;
                            if (image.Name == current_hovered_image)
                            {
                                Canvas.SetTop(image, Canvas.GetTop(image) - 0.5);
                            }
                        }
                    }
                    break;
                case Key.Down:
                    // Offset -= 1, SetTop += 1 (GD and WPF have different y axis.
                    offset_y -= 1;
                    foreach (UIElement child in canvas.Children)
                    {
                        if (child is Image)
                        {
                            Image image = (Image)child;
                            if (image.Name == current_hovered_image)
                            {
                                Canvas.SetTop(image, Canvas.GetTop(image) + 0.5);
                            }
                        }
                    }
                    break;
                case Key.Left:
                    offset_x -= 1;
                    foreach (UIElement child in canvas.Children)
                    {
                        if (child is Image)
                        {
                            Image image = (Image)child;
                            if (image.Name == current_hovered_image)
                            {
                                Canvas.SetLeft(image, Canvas.GetLeft(image) - 0.5);
                            }
                        }
                    }
                    break;
                case Key.Right:
                    offset_x += 1;
                    foreach (UIElement child in canvas.Children)
                    {
                        if (child is Image)
                        {
                            Image image = (Image)child;
                            if (image.Name == current_hovered_image)
                            {
                                Canvas.SetLeft(image, Canvas.GetLeft(image) + 0.5);
                            }
                        }
                    }
                    break;
                case Key.OemOpenBrackets:
                    source_x -= 1;
                    foreach (UIElement child in canvas.Children)
                    {
                        if (child is Image)
                        {
                            Image image = (Image)child;
                            if (image.Name == current_hovered_image)
                            {
                                image.Width -= 1;
                            }
                        }
                    }
                    break;
                case Key.OemCloseBrackets:
                    source_x += 1;
                    foreach (UIElement child in canvas.Children)
                    {
                        if (child is Image)
                        {
                            Image image = (Image)child;
                            if (image.Name == current_hovered_image)
                            {
                                image.Width += 1;
                            }
                        }
                    }
                    break;
                case Key.OemMinus:
                    source_y -= 1;
                    foreach (UIElement child in canvas.Children)
                    {
                        if (child is Image)
                        {
                            Image image = (Image)child;
                            if (image.Name == current_hovered_image)
                            {
                                image.Height -= 1;
                            }
                        }
                    }
                    break;
                case Key.OemPlus:
                    source_y += 1;
                    foreach (UIElement child in canvas.Children)
                    {
                        if (child is Image)
                        {
                            Image image = (Image)child;
                            if (image.Name == current_hovered_image)
                            {
                                image.Height += 1;
                            }
                        }
                    }
                    break;
                case Key.Enter:
                    // signifies it's time to stop hovering an image and saves current data..
                    if (current_hovered_image != "")
                    {
                        if (!gMenuChanges.Contains(current_hovered_image))
                        {
                            int[] data = { offset_x, offset_y, source_x, source_y };
                            gMenuChanges.Add(current_hovered_image, data);
                        }
                        else
                        {
                            int[] data = { offset_x, offset_y, source_x, source_y };
                            gMenuChanges[current_hovered_image] = data;
                        }
                        offset_x = 0;
                        offset_y = 0;
                        source_x = 0;
                        source_y = 0;
                        foreach (UIElement child in canvas.Children)
                        {
                            if (child is Image)
                            {
                                Image image = (Image)child;
                                if (image.Name == current_hovered_image)
                                {
                                    image.Opacity = 1.0;
                                }
                            }
                        }
                        current_hovered_image = "";
                    }
                    break;
                default:
                    break;
            }
        }
        private void Image_MouseDown(object sender, MouseEventArgs e)
        {
            Image image = sender as Image;
            if (image == null)
            {
                return;
            }
            // Check if there is a different image currently selected and save the modified data:
            if (current_hovered_image != "")
            {
                if (!gMenuChanges.Contains(current_hovered_image))
                {
                    int[] data = { offset_x, offset_y, source_x, source_y };
                    gMenuChanges.Add(current_hovered_image, data);
                }
                else
                {
                    int[] data = { offset_x, offset_y, source_x, source_y };
                    gMenuChanges[current_hovered_image] = data;
                }
            }
            foreach (UIElement child in canvas.Children)
            {
                if (child is Image)
                {
                    Image i = (Image)child;
                    i.Opacity = 1.0;
                }
            }

            // Signify the image is selected.
            image.Opacity = 0.5;

            // Set global variables to work in tandum with Window_KeyDown.
            offset_x = 0;
            offset_y = 0;
            source_x = 0;
            source_y = 0;
            current_hovered_image = image.Name;
        }
        public void Unset_Workspace(object sender, RoutedEventArgs e)
        {
            BitmapImage bmi = new BitmapImage();
            bmi.BeginInit();
            bmi.UriSource = new Uri(@"/WorkspaceImages/Blank.png", UriKind.RelativeOrAbsolute);
            bmi.EndInit();
            WorkSpace.Source = bmi;
            mainGrid.Children.Remove(canvas);
            mainmenu.IsChecked = false;
            canvas.Children.RemoveRange(0, canvas.Children.Count - 1);
            if (!canvas.Children.Contains(WorkSpace))
            {
                canvas.Children.Add(WorkSpace);
            }
        }
        public void Set_MainMenu(object sender, RoutedEventArgs e)
        {
            if (gProjectLocation == "")
            {
                MessageBox.Show("Please create a project first. To create a project go to `File > New Project`.", "Set Main Menu Error", MessageBoxButton.OK, MessageBoxImage.Error);
                mainmenu.IsChecked = false;
                return;
            }
            BitmapImage bmi = new BitmapImage();
            bmi.BeginInit();
            bmi.UriSource = new Uri(@"/WorkspaceImages/MainMenu.png", UriKind.RelativeOrAbsolute);
            bmi.EndInit();
            WorkSpace.Source = bmi;

            canvas.Width = WorkSpace.Width;
            canvas.Height = WorkSpace.Height;
            int menuOffset = 18;
            StreamReader plist;
            Hashtable frames = new Hashtable();

            // TODO:
            // In future add a "set mode" between uhd, hd, and non hd.
            // For now working in only uhd.

            // switch { UHD: ...
            // GameSheet03-uhd
            bool gameSheet03_exists = false;
            if (Directory.Exists(gProjectLocation + @"/GJ_GameSheet03-uhd"))
            {
                gameSheet03_exists = true;
            }

            if (gameSheet03_exists)
            {
                plist = new StreamReader(gProjectLocation + @"/GJ_GameSheet03-uhd.plist");
                frames = read_plist(plist);
            }

            add_image_to_canvas(ref canvas, "GJ_closeBtn_001.png", "GJ_GameSheet03-uhd", frames, gameSheet03_exists, 42, 42, menuOffset);
            add_image_to_canvas(ref canvas, "GJ_garageBtn_001.png", "GJ_GameSheet03-uhd", frames, gameSheet03_exists, 396, 341, menuOffset);
            add_image_to_canvas(ref canvas, "GJ_profileButton_001.png", "GJ_GameSheet03-uhd", frames, gameSheet03_exists, 103, 487, menuOffset);
            add_image_to_canvas(ref canvas, "gj_ytIcon_001.png", "GJ_GameSheet03-uhd", frames, gameSheet03_exists, 180, 597, menuOffset);
            add_image_to_canvas(ref canvas, "gj_twitchIcon_001.png", "GJ_GameSheet03-uhd", frames, gameSheet03_exists, 245, 597, menuOffset);
            add_image_to_canvas(ref canvas, "gj_twIcon_001.png", "GJ_GameSheet03-uhd", frames, gameSheet03_exists, 115, 597, menuOffset);
            add_image_to_canvas(ref canvas, "gj_fbIcon_001.png", "GJ_GameSheet03-uhd", frames, gameSheet03_exists, 50, 597, menuOffset);
            add_image_to_canvas(ref canvas, "gj_discordIcon_001.png", "GJ_GameSheet03-uhd", frames, gameSheet03_exists, 245, 662, menuOffset);
            add_image_to_canvas(ref canvas, "robtoplogo_small.png", "GJ_GameSheet03-uhd", frames, gameSheet03_exists, 112, 663, menuOffset);
            add_image_to_canvas(ref canvas, "GJ_achBtn_001.png", "GJ_GameSheet03-uhd", frames, gameSheet03_exists, 449, 621, menuOffset);
            add_image_to_canvas(ref canvas, "GJ_optionsBtn_001.png", "GJ_GameSheet03-uhd", frames, gameSheet03_exists, 574, 621, menuOffset);
            add_image_to_canvas(ref canvas, "GJ_statsBtn_001.png", "GJ_GameSheet03-uhd", frames, gameSheet03_exists, 699, 621, menuOffset);
            add_image_to_canvas(ref canvas, "GJ_ngBtn_001.png", "GJ_GameSheet03-uhd", frames, gameSheet03_exists, 829, 621, menuOffset);
            add_image_to_canvas(ref canvas, "GJ_dailyRewardBtn_001.png", "GJ_GameSheet03-uhd", frames, gameSheet03_exists, 1190, 319, menuOffset);

            bool gameSheet04_exists = false;
            if (Directory.Exists(gProjectLocation + @"/GJ_GameSheet04-uhd"))
            {
                gameSheet04_exists = true;
            }

            if (gameSheet04_exists)
            {
                plist = new StreamReader(gProjectLocation + @"/GJ_GameSheet04-uhd.plist");
                frames = read_plist(plist);
            }

            add_image_to_canvas(ref canvas, "GJ_playBtn_001.png", "GJ_GameSheet04-uhd", frames, gameSheet04_exists, 646, 343, menuOffset);
            add_image_to_canvas(ref canvas, "GJ_creatorBtn_001.png", "GJ_GameSheet04-uhd", frames, gameSheet04_exists, 891, 341, menuOffset);
            add_image_to_canvas(ref canvas, "GJ_moreGamesBtn_001.png", "GJ_GameSheet04-uhd", frames, gameSheet04_exists, 1184, 622, menuOffset);

            bool launchSheet_exists = false;
            if (Directory.Exists(gProjectLocation + @"/GJ_LaunchSheet-uhd"))
            {
                launchSheet_exists = true;
            }

            if (launchSheet_exists)
            {
                plist = new StreamReader(gProjectLocation + @"/GJ_LaunchSheet-uhd.plist");
                frames = read_plist(plist);
            }

            add_image_to_canvas(ref canvas, "GJ_logo_001.png", "GJ_LaunchSheet-uhd", frames, launchSheet_exists, 643, 116, menuOffset);

            mainGrid.Children.Add(canvas);

        }
        public void Set_OnlineMenu(object sender, RoutedEventArgs e)
        {
            return;
        }
    }
}