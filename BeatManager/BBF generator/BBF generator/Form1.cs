using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Web;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;


namespace BBF_generator
{
    public partial class frmGenerator : Form
    {
       // JsonTextParser parser;
        int duration;
        public frmGenerator()
        {
            InitializeComponent();
            //parser = new JsonTextParser();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            duration = -10;
        }

        public List<int> UploadTrack(Stream binaryData, string postToUrl)
        {
            System.Net.ServicePointManager.Expect100Continue = false;
            List<int> beats = null;
            // make http request
            var request = (HttpWebRequest)WebRequest.Create(postToUrl);
            request.Method = "POST";
            request.ContentType = "application/octet-stream"; // binary data: 

            // data (bytes) that will be posted in body of request
            var streamOut = request.GetRequestStream();
            binaryData.CopyTo(streamOut);

            // post and get response
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var code = response.StatusCode;
              //string jsonResponse =  "{\"response\": {\"status\": {\"version\": \"4.2\", \"code\": 0, \"message\": \"Success\"}, \"track\": {\"status\": \"complete\", \"audio_md5\": \"02bd76c2d7a51ac5c7f3ead7b16df178\", \"title\": \"\", \"artist\": \"www.SoundJay.com\", \"analyzer_version\": \"3.01a\", \"release\": \"\", \"samplerate\": 44100, \"bitrate\": 128, \"id\": \"TRQJHNO12E7B8A567A\", \"md5\": \"5bcdd71bfc90772561ae6d87dc84b0f9\"}}}"; 
                
                    string jsonResponse = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    //txtFile1.Text = "" + (int)code;

                UploadResponse upload = new UploadResponse(jsonResponse);
                if (upload.isSetup())
                {
                    string wsUrl = "http://developer.echonest.com/api/v4/track/analyze";
                    string postData = string.Format("api_key={0}&wait=true&format=json&bucket=audio_summary", SharedData.APIKey);


                        postData += string.Format("&md5={0}", upload.GetMD5());

                    SharedData.PerformPostMultipartRequest(wsUrl, postData);

                    string res = SharedData.ReadWebRequestResult();
                    //txtFile2.Text = res;

                    AnalyseResponse analyse = new AnalyseResponse(res);
                    if (analyse.isSetup())
                    {
                        if (duration < 0)
                        {
                            duration = analyse.GetDuration();
                        }
                        else
                        {
                            if(duration != analyse.GetDuration()) {
                                throw new NotImplementedException();
                            }
                        }
                        beats = analyse.GetBeats();

                    }
                    
                }

                  
                return beats;
                //    return 0;
            }

        }
        private Boolean setupRight()
        {
            for (int i = 0; i < noOfLayers.Value; i++)
            {
                TextBox curText = (TextBox)this.Controls["txtFile" + i.ToString()];
                if(curText.Text == "") return false;
                if (txtMP3.Text == "") return false;
            }
            return true;
        }

        private void WriteBFF(List<Beat>[] beats, int duration)
        {
            // Displays a SaveFileDialog so the user can save the Image
            // assigned to Button2.
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "BFF File|*.bff";
            saveFileDialog1.Title = "Save the BFF File";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.
                StreamWriter fs =
                  new StreamWriter(saveFileDialog1.OpenFile());

                fs.WriteLine("#{[Title: N/A], [Gamertag: N/A], [Theme: N/A], [Size: N/A], [Note: This is sample metadata, and doesnt change}#");
                fs.WriteLine("<"+beats.Length+">");
                fs.WriteLine("<" + duration + ">");
                int[] offsets = new int[beats.Length];
                for (int i = 0; i < beats.Length; i++)
                {
                    offsets[i] = 0;
                }
                for (int i =0;i<duration;i++) {
                    List<int> layers = new List<int>();
                    List<char> buttons = new List<char>();
                    for(int j = 0;j<beats.Length;j++){
                        if(offsets[j]<beats[j].Count && (beats[j].ElementAt(offsets[j])).getTime()==i) {
                            layers.Add(j + 1);
                            buttons.Add((beats[j].ElementAt(offsets[j])).getKey());
                            offsets[j]++;
                        }
                    }
                    if (layers.Count != 0)
                    {
                        string line = "{"+i+"} {";
                        foreach (int layer in layers)
                        {
                            line += layer + ",";
                        }
                        line = line.Substring(0, line.Length - 1) + "} {";
                        foreach (char button in buttons)
                        {
                            line += button + ",";
                        }
                        line = line.Substring(0, line.Length - 1) + "} ";
                        fs.WriteLine(line);

                    }
                }

                fs.Close();
            }
        }

        public List<Beat> GetButtons(Stream file)
        {
            StreamReader reader = new StreamReader(file);
            List<Beat> result = new List<Beat>();
            string line = reader.ReadLine();
            string[] parts = line.Split(',');
            float tempo=0.0f;
            float division=0.0f;
            while (!parts[2].StartsWith(" Note"))
            {
                switch (parts[2])
                {
                    case " Tempo":
                        tempo = int.Parse(parts[3])*0.001f;
                        break;
                    case " Header":
                        division = int.Parse(parts[5]);
                        break;
                    default:
                        break;
                }                
                line = reader.ReadLine();
                parts = line.Split(',');
            }
            float ratio = division/tempo;
            while (!parts[2].StartsWith(" End_tra"))
            {
                if (parts[2].StartsWith(" Note"))
                {
                    int temp = (int)(int.Parse(parts[1]) / ratio);
                    if (int.Parse(parts[5]) != 0)
                    {
                        switch (int.Parse(parts[4]))
                        {
                            case 24: //C1
                                result.Add(new Beat(temp, 'X'));
                                break;
                            case 26: //D1
                                result.Add(new Beat(temp, 'Y'));
                                break;
                            case 33: //A1
                                result.Add(new Beat(temp, 'A'));
                                break;
                            case 35: //B1
                                result.Add(new Beat(temp, 'B'));
                                break;
                            default:
                                result.Add(new Beat(temp, 'A'));
                                break;
                        }
                        //result.Add(temp);
                    }
                }
                line = reader.ReadLine();
                parts = line.Split(',');
            }

            while ((line) != null)
            {

                parts = line.Split(','); 
                if (parts[2].StartsWith(" End_tra"))
                {
             //       duration = (int)(int.Parse(parts[1]) / ratio);
                }
                line = reader.ReadLine();
                
                //1, 45785, End_track
            }

            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (setupRight())
            {
                List<Beat> []beats = new List<Beat>[(int)noOfLayers.Value];

                for (int i = 0; i < noOfLayers.Value; i++)
                {
                    TextBox curText = (TextBox)this.Controls["txtFile" + i.ToString()];
                    FileStream fileStream = new FileStream(curText.Text, FileMode.Open);
                    string filetype = Path.GetExtension(curText.Text);
                    filetype = filetype.Substring(1,filetype.Length-1);
                    beats[i] = GetButtons(fileStream);
                    fileStream.Close();
                }
                FileStream fileStream2 = new FileStream(txtMP3.Text, FileMode.Open);
                string filetype2 = Path.GetExtension(txtMP3.Text);
                filetype2 = filetype2.Substring(1, filetype2.Length - 1);
                UploadTrack(fileStream2, "http://developer.echonest.com/api/v4/track/upload?api_key=DDFQ4IGNU8RKVHRKW&filetype=" + filetype2);
                //duration = 41142;
                fileStream2.Close();

                WriteBFF(beats, duration);
            }
            //FileStream fileStream = new FileStream(txtFile0.Text, FileMode.Open);

            //UploadTrack(fileStream, "http://developer.echonest.com/api/v4/track/upload?api_key=DDFQ4IGNU8RKVHRKW&filetype=mp3");

            //fileStream.Close();
        }

        private void noOfLayers_ValueChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < noOfLayers.Value; i++)
            {
                TextBox curText = (TextBox)this.Controls["txtFile" + i.ToString()];
                Button curButton = (Button)this.Controls["btnBrowse" + i.ToString()];
                curText.Visible = true;
                curButton.Visible = true;

            }

            for (int i = (int)noOfLayers.Value; i <5; i++)
            {
                TextBox curText = (TextBox)this.Controls["txtFile" + i.ToString()];
                Button curButton = (Button)this.Controls["btnBrowse" + i.ToString()];
                curText.Visible = false;
                curButton.Visible = false;

            }
        }

        private void btnBrowse0_Click(object sender, EventArgs e)
        {
            txtFile0.Text = SelectTextFile();
        }

        private void btnBrowse1_Click(object sender, EventArgs e)
        {
            txtFile1.Text = SelectTextFile();
        }

        private void btnBrowse2_Click(object sender, EventArgs e)
        {
            txtFile2.Text = SelectTextFile();
        }

        private void btnBrowse3_Click(object sender, EventArgs e)
        {
            txtFile3.Text = SelectTextFile();
        }

        private void btnBrowse4_Click(object sender, EventArgs e)
        {
            txtFile4.Text = SelectTextFile();
        }

        private string SelectTextFile(/*string initialDirectory*/)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter =
               "button files (*.btn)|*.btn";
            //dialog.InitialDirectory = initialDirectory;
            dialog.Title = "Select a base file";
            return (dialog.ShowDialog() == DialogResult.OK)
               ? dialog.FileName : null;
        }

        private string SelectTextFile2(/*string initialDirectory*/)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter =
               "audio files (*.wav,*.mp3)|*.wav;*.mp3";
            //dialog.InitialDirectory = initialDirectory;
            dialog.Title = "Select a base file";
            return (dialog.ShowDialog() == DialogResult.OK)
               ? dialog.FileName : null;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            txtMP3.Text = SelectTextFile2();
        }
    }
}
