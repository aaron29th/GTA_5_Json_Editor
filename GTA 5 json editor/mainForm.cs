using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using PS3Lib;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Win32;
using System.Drawing.Imaging;
using System.Threading;

namespace GTA_5_json_editor
{
    public partial class mainForm : DevExpress.XtraEditors.XtraForm
    {
        public static PS3API PS3 = new PS3API(SelectAPI.TargetManager);

        private void checkUpdates()
        {
            // Check if server is online
            if (new Ping().Send("aaronrosser.xyz").Status == IPStatus.Success)
            {
                try
                {
                    HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(Variables.versionCheckURL);
                    Request.UserAgent = Variables.version.ToString();
                    var Response = Request.GetResponse().GetResponseStream();
                    using (StreamReader sr = new StreamReader(Response))
                    {
                        Dictionary<string, object> jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(sr.ReadToEnd());

                        if (jsonData.ContainsKey("version"))
                        {
                            Variables.lastestVersion = Convert.ToInt32(jsonData["version"]);
                        }

                        if (jsonData.ContainsKey("lastestDownload"))
                        {
                            Variables.lastestDownloadLink = Convert.ToString(jsonData["lastestDownload"]);
                        }

                        if (jsonData.ContainsKey("startUpMessage") && jsonData["startUpMessage"] != null)
                        {
                            output(jsonData["startUpMessage"]);
                        }
                    }
                } catch
                {
                    output("Failed to check for updates");
                }
            }
            else
            {
                output("Failed to check for updates");
            }

            if (Variables.version < Variables.lastestVersion)
            {
                output("There is a new version available" + Environment.NewLine + "Go to: " + Variables.lastestDownloadLink + " to download the lastest version");
            }
        }

        private bool isValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    output(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    output(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void registryKeyAdd(string keyName, object value)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

            key.CreateSubKey("rosser");
            key = key.OpenSubKey("rosser", true);

            key.CreateSubKey("GTA_JSON_Editor");
            key = key.OpenSubKey("GTA_JSON_Editor", true);

            key.SetValue(keyName, value);
        }

        private object registyKeyGet(string keyName)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

            key.CreateSubKey("rosser");
            key = key.OpenSubKey("rosser", true);

            key.CreateSubKey("GTA_JSON_Editor");
            key = key.OpenSubKey("GTA_JSON_Editor", true);

            if (key.GetValue(keyName) == null)
            {
                return "";
            } 
            return key.GetValue(keyName);
        }

        private void output(object text)
        {
            outputBox.Text = "[ " + DateTime.Now.ToString("H:mm:ss") + " ] " + Convert.ToString(text) + Environment.NewLine + outputBox.Text;
        }

        private int preload_find()
        {
            return RPC.Call(Natives.PRELOAD_FIND);
        }

        public mainForm()
        {
            InitializeComponent();
            checkUpdates();
            output("Created by @AaronROsser");
            output("JSON Editor 1.50");

            customMetaJobId.Text = registyKeyGet("jobId").ToString();
        }

        #region Buttons
        private void connect_Click(object sender, EventArgs e)
        {
            if (PS3.ConnectTarget()) output("Connect to " + PS3.GetConsoleName());
            else output("Failed to connect");
        }

        private void attach_Click(object sender, EventArgs e)
        {
            if (PS3.ConnectTarget())
            {
                if (PS3.AttachProcess())
                {
                    output("Attached");
                    RPC.Enable();
                    output("RPC enabled");
                }
                else
                {
                    output("Failed to attach");
                }
            } else
            {
                output("Not connected :(");
            }
        }

        private void datafileCreate_Click(object sender, EventArgs e)
        {
            output("Datafile create | "  + RPC.Call(Natives.DATAFILE_CREATE));
        }

        private void datafileDelete_Click(object sender, EventArgs e)
        {
            output("Datafile delete | " + RPC.Call(Natives.DATAFILE_DELETE));
        }

        private void preloadFind_Click(object sender, EventArgs e)
        {
            output("Preload find | " + RPC.Call(Natives.PRELOAD_FIND));
        }

        private void loadUgcFile_Click(object sender, EventArgs e)
        {
            output("Loaded UGC file | " + RPC.Call(Natives._LOAD_UGC_FILE, ugcFileName.Text));
        }

        #region Common objects
        private void commonCopyRoot_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(preload_find().ToString());
        }

        private void commonCopyMission_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, preload_find(), "mission").ToString());
        }

        private void commonCopyGen_Click(object sender, EventArgs e)
        {
            int missionObject = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, preload_find(), "mission");
            Clipboard.SetText(RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, missionObject, "gen").ToString());
        }

        private void commonCopyProp_Click(object sender, EventArgs e)
        {
            int missionObject = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, preload_find(), "mission");
            Clipboard.SetText(RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, missionObject, "prop").ToString());
        }

        private void commonCopyMeta_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, preload_find(), "meta").ToString());
        }

        private void commonCopyRule_Click(object sender, EventArgs e)
        {
            int missionObject = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, preload_find(), "mission");
            Clipboard.SetText(RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, missionObject, "rule").ToString());
        }

        private void commonCopyEndcon_Click(object sender, EventArgs e)
        {
            int missionObject = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, preload_find(), "mission");
            Clipboard.SetText(RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, missionObject, "endcon").ToString());
        }

        private void commonCopyVeh_Click(object sender, EventArgs e)
        {
            int missionObject = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, preload_find(), "mission");
            Clipboard.SetText(RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, missionObject, "veh").ToString());
        }
        #endregion

        #region Object Get
        private void objectGetBool_Click(object sender, EventArgs e)
        {
            int value = RPC.Call(Natives._OBJECT_VALUE_GET_BOOLEAN, (int)itemContainerBool.Value, itemKeyBool.Text);
            itemValueBool.Text = Convert.ToBoolean(value) ? "True" : "False";
            output("Object get bool | " + itemContainerBool.Value + " | " + itemKeyBool.Text + " | " + value);
        }

        private void objectGetInt_Click(object sender, EventArgs e)
        {
            int value = RPC.Call(Natives._OBJECT_VALUE_GET_INTEGER, (int)itemContainerInt.Value, itemKeyInt.Text);
            itemValueInt.Value = value;
            output("Object get int | " + itemContainerInt.Value + " | " + itemKeyInt.Text + " | " + value);
        }

        private void objectGetFloat_Click(object sender, EventArgs e)
        {
            float value = RPC.Call(Natives._OBJECT_VALUE_GET_FLOAT, (int)itemContainerFloat.Value, itemKeyFloat.Text);
            itemValueFloat.Value = (decimal)value;
            output("Object get float | " + itemContainerFloat.Value + " | " + itemKeyFloat.Text + " | " + value);
        }

        private void objectGetString_Click(object sender, EventArgs e)
        {
            uint location = (uint)RPC.Call(Natives._OBJECT_VALUE_GET_STRING, (int)itemContainerString.Value, itemKeyString.Text);
            string value = PS3.Extension.ReadString(location);
            itemValueString.Text = value;
            output("Object get string | " + itemContainerString.Value + " | " + itemKeyString.Text + " | " + value);
        }

        private void objectGetVector_Click(object sender, EventArgs e)
        {
            int number = preload_find();
            Console.Write(RPC.Call(Natives._OBJECT_VALUE_GET_VECTOR3, (int)itemContainerVector.Value, itemKeyVector.Text));
        }

        private void objectGetObject_Click(object sender, EventArgs e)
        {
            int value = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, (int)itemContainerObject.Value, itemKeyObject.Text);
            itemValueObject.Value = value;
            output("Object get object | " + itemContainerObject.Value + " | " + itemKeyObject.Text + " | " + value);
        }

        private void objectGetArray_Click(object sender, EventArgs e)
        {
            int value = RPC.Call(Natives._OBJECT_VALUE_GET_ARRAY, (int)itemContainerArray.Value, itemKeyArray.Text);
            itemValueArray.Value = value;
            output("Object get array | " + itemContainerArray.Value + " | " + itemKeyArray.Text + " | " + value);
        }
        #endregion

        #region Object Add
        private void objectAddBool_Click(object sender, EventArgs e)
        {
            int value = 0;
            if (itemValueBool.Text == "True") value = 1;
            int returnedValue = RPC.Call(Natives._OBJECT_VALUE_ADD_BOOLEAN, (int)itemContainerBool.Value, itemKeyBool.Text, value);
            output("Object add bool | " + itemContainerBool.Value + " | " + itemKeyBool.Text + " | " + itemValueBool.Text + " | " + returnedValue);
        }

        private void objectAddInt_Click(object sender, EventArgs e)
        {
            int value = RPC.Call(Natives._OBJECT_VALUE_ADD_INTEGER, (int)itemContainerInt.Value, itemKeyInt.Text, (int)itemValueInt.Value);
            output("Object add int | " + itemContainerInt.Value + " | " + itemKeyInt.Text + " | " + itemValueInt.Value + " | " + value);
        }

        private void objectAddFloat_Click(object sender, EventArgs e)
        {
            int value = RPC.Call(Natives._OBJECT_VALUE_ADD_FLOAT, (int)itemContainerFloat.Value, itemKeyFloat.Text, (float)itemValueFloat.Value);
            output("Object add float | " + itemContainerFloat.Value + " | " + itemKeyFloat.Text + " | " + itemValueFloat.Value + " | " + value);
        }

        private void objectAddString_Click(object sender, EventArgs e)
        {
            int value = RPC.Call(Natives._OBJECT_VALUE_ADD_STRING, (int)itemContainerString.Value, itemKeyString.Text, itemValueString.Text);
            output("Object add string | " + itemContainerString.Value + " | " + itemKeyString.Text + " | " + itemValueString.Text + " | " + value);
        }

        private void objectAddVector_Click(object sender, EventArgs e)
        {

        }

        private void objectAddObject_Click(object sender, EventArgs e)
        {
            int value = RPC.Call(Natives._OBJECT_VALUE_ADD_OBJECT, (int)itemContainerObject.Value, itemKeyObject.Text);
            output("Object add object | " + itemContainerObject.Value + " | " + itemKeyObject.Text + " | " + value);
        }

        private void objectAddArray_Click(object sender, EventArgs e)
        {
            int value = RPC.Call(Natives._OBJECT_VALUE_ADD_ARRAY, (int)itemContainerArray.Value, itemKeyArray.Text);
            output("Object add array | " + itemContainerArray.Value + " | " + itemKeyArray.Text + " | " + value);
        }


        #endregion

        #region Array Get
        private void arrayGetBool_Click(object sender, EventArgs e)
        {
            int value = RPC.Call(Natives._ARRAY_VALUE_GET_BOOLEAN, (int)arrayAddressBool.Value, (int)arrayIndexBool.Value);
            arrayValueBool.Text = Convert.ToBoolean(value) ? "True" : "False";
            output("Array get bool | " + arrayAddressBool.Value + " | " + arrayIndexBool.Text + " | " + value);
        }

        private void arrayGetInt_Click(object sender, EventArgs e)
        {
            int value = RPC.Call(Natives._ARRAY_VALUE_GET_INTEGER, (int)arrayAddressInt.Value, (int)arrayIndexInt.Value);
            arrayValueInt.EditValue = value;
            output("Array get int | " + arrayAddressInt.Value + " | " + arrayIndexInt.Text + " | " + value);
        }

        private void arrayGetFloat_Click(object sender, EventArgs e)
        {
            int value = RPC.Call(Natives._ARRAY_VALUE_GET_FLOAT, (int)arrayAddressFloat.Value, (int)arrayIndexFloat.Value);
            arrayValueFloat.Value = value;
            output("Array get float | " + arrayAddressFloat.Value + " | " + arrayIndexFloat.Text + " | " + value);
        }

        private void arrayGetString_Click(object sender, EventArgs e)
        {
            uint location = (uint)RPC.Call(Natives._ARRAY_VALUE_GET_STRING, (int)arrayAddressString.Value, (int)arrayIndexString.Value);
            string value = PS3.Extension.ReadString(location);
            arrayValueString.Text = value;
            output("Array get string | " + arrayAddressString.Value + " | " + arrayIndexString.Text + " | " + value);
        }

        private void arrayGetVector_Click(object sender, EventArgs e)
        {

        }

        private void arrayGetObject_Click(object sender, EventArgs e)
        {
            int value = RPC.Call(Natives._ARRAY_VALUE_GET_OBJECT, (int)arrayAddressObject.Value, (int)arrayIndexObject.Value);
            arrayValueObject.Value = value;
            output("Array get object | " + arrayAddressObject.Value + " | " + arrayIndexObject.Text + " | " + value);
        }
        #endregion

        #region Array Add
        private void arrayAddBool_Click(object sender, EventArgs e)
        {
            bool value = false;
            if (arrayValueBool.Text == "True") value = true;
            int returnedValue = RPC.Call(Natives._ARRAY_VALUE_ADD_INTEGER, (int)arrayAddressBool.Value, value);
            output("Array add bool | " + arrayAddressBool.Value + " | " + arrayAddressBool.Text + " | " + value + " | " + returnedValue);
        }

        private void arrayAddInt_Click(object sender, EventArgs e)
        {
            int value = RPC.Call(Natives._ARRAY_VALUE_ADD_INTEGER, (int)arrayAddressInt.Value, (int)arrayValueInt.Value);
            output("Array add int | " + arrayAddressInt.Value + " | " + arrayIndexInt.Value + " | " + arrayValueInt.Value + " | " + value);
        }

        private void arrayAddFloat_Click(object sender, EventArgs e)
        {
            int value = RPC.Call(Natives._ARRAY_VALUE_ADD_FLOAT, (int)arrayAddressFloat.Value, (float)arrayValueFloat.Value);
            output("Array add float | " + arrayAddressFloat.Value + " | " + arrayIndexFloat.Value + " | " + arrayValueFloat.Value + " | " + value);
        }

        private void arrayAddString_Click(object sender, EventArgs e)
        {
            int value = RPC.Call(Natives._ARRAY_VALUE_ADD_STRING, (int)arrayAddressString.Value, arrayValueString.Text);
            output("Array add string | " + arrayAddressString.Value + " | " + arrayIndexString.Value + " | " + arrayValueString.Text + " | " + value);
        }

        private void arrayAddVector_Click(object sender, EventArgs e)
        {
            
        }

        private void arrayAddObject_Click(object sender, EventArgs e)
        {
            int value = RPC.Call(Natives._ARRAY_VALUE_ADD_OBJECT, (int)arrayAddressObject.Value);
            output("Array add object | " + arrayAddressObject.Value + " | " + arrayIndexObject.Value + " | " + arrayValueObject.Value + " | " + value);
        }
        #endregion

        private void devModeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (devModeCheckBox.Checked)
            {
                PS3.Extension.WriteUInt32(0x3E30D8, 0x38600001);
                output("Dev mode enabled");
            } else
            {
                PS3.Extension.WriteUInt32(0x3E30D8, 0x60830000);
                output("Dev mode disabled");
            }
        }

        private void jobCopyGet_Click(object sender, EventArgs e)
        {
            jobCopyId.Text = menu.getJobId((int)jobCopyNumber.Value, (bool)jobCopyType.EditValue);
        }

        private void jobCopySet_Click(object sender, EventArgs e)
        {
            menu.setJobId((int)jobCopyNumber.Value, jobCopyId.Text, (bool)jobCopyType.EditValue);
        }

        #region In game editor
        private void inGameNameGet_Click(object sender, EventArgs e)
        {
            inGameName.Text = JobDetails.title;
        }

        private void inGameNameSet_Click(object sender, EventArgs e)
        {
            JobDetails.title = inGameName.Text;
        }

        private void inGameTypeGet_Click(object sender, EventArgs e)
        {
            inGameType.Value = JobDetails.type;
        }

        private void inGameTypeSet_Click(object sender, EventArgs e)
        {
            JobDetails.type = (int)inGameType.Value;
        }

        private void inGameDecGet_Click(object sender, EventArgs e)
        {
            inGameDec.Text = JobDetails.description;
        }

        private void inGameDecSet_Click(object sender, EventArgs e)
        {
            JobDetails.description = inGameDec.Text;
        }

        private void inGameSubtypeMissionGet_Click(object sender, EventArgs e)
        {
            inGameSubTypeMission.Value = JobDetails.missionSubtype;
        }

        private void inGameSubtypeMissionSet_Click(object sender, EventArgs e)
        {
            JobDetails.missionSubtype = (int)inGameSubTypeMission.Value;
        }

        private void inGameSubtypeRaceGet_Click(object sender, EventArgs e)
        {
            inGameSubtypeRace.Value = JobDetails.raceType;
        }

        private void inGameSubtypeRaceSet_Click(object sender, EventArgs e)
        {
            JobDetails.raceType = (int)inGameSubtypeRace.Value;
        }

        private void inGameSubtypeDeathmatchGet_Click(object sender, EventArgs e)
        {
            inGameSubtypeDeathmatch.Value = JobDetails.deathmatchType;
        }

        private void inGameSubtypeDeathmatchSet_Click(object sender, EventArgs e)
        {
            JobDetails.deathmatchType = (int)inGameSubtypeDeathmatch.Value;
        }

        #endregion

        #region Quick json editor
        private void quickJsonTypeGet_Click(object sender, EventArgs e)
        {
            int root = preload_find();
            int mission = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, root, "mission");
            int gen = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, mission, "gen");
            quickJsonType.Value = RPC.Call(Natives._OBJECT_VALUE_GET_INTEGER, gen, "type");
            output("Get type | " + quickJsonType.Value);
        }

        private void quickJsonTypeSet_Click(object sender, EventArgs e)
        {
            int root = preload_find();
            int mission = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, root, "mission");
            int gen = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, mission, "gen");
            RPC.Call(Natives._OBJECT_VALUE_ADD_INTEGER, gen, "type", (int)quickJsonType.Value);
            output("Set type | " + quickJsonType.Value);
        }

        private void quickJsonSubtypeGet_Click(object sender, EventArgs e)
        {
            int root = preload_find();
            int mission = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, root, "mission");
            int gen = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, mission, "gen");
            quickJsonSubtype.Value = RPC.Call(Natives._OBJECT_VALUE_GET_INTEGER, gen, "subtype");
            output("Get subtype | " + quickJsonSubtype.Value);
        }

        private void quickJsonSubtypeSet_Click(object sender, EventArgs e)
        {
            int root = preload_find();
            int mission = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, root, "mission");
            int gen = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, mission, "gen");
            RPC.Call(Natives._OBJECT_VALUE_ADD_INTEGER, gen, "subtype", (int)quickJsonSubtype.Value);
            output("Set subtype | " + quickJsonSubtype.Value);
        }

        private void quickJsonOptbsGet_Click(object sender, EventArgs e)
        {
            int root = preload_find();
            int mission = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, root, "mission");
            int gen = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, mission, "gen");
            quickJsonOptbs.Value = RPC.Call(Natives._OBJECT_VALUE_GET_INTEGER, gen, "optbs");
            output("Get optbs | " + quickJsonOptbs.Value);
        }

        private void quickJsonOptsbsSet_Click(object sender, EventArgs e)
        {
            int root = preload_find();
            int mission = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, root, "mission");
            int gen = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, mission, "gen");
            RPC.Call(Natives._OBJECT_VALUE_ADD_INTEGER, gen, "optbs", (int)quickJsonOptbs.Value);
            output("Set optbs | " + quickJsonOptbs.Value);
        }

        private void quickJsonNameGet_Click(object sender, EventArgs e)
        {
            int root = preload_find();
            int mission = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, root, "mission");
            int gen = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, mission, "gen");
            uint location = (uint)RPC.Call(Natives._OBJECT_VALUE_GET_STRING, gen, "nm");
            quickJsonName.Text = PS3.Extension.ReadString(location);
            output("Get title | " + quickJsonName.Text);
        }

        private void quickJsonNameSet_Click(object sender, EventArgs e)
        {
            int root = preload_find();
            int mission = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, root, "mission");
            int gen = RPC.Call(Natives._OBJECT_VALUE_GET_OBJECT, mission, "gen");
            RPC.Call(Natives._OBJECT_VALUE_ADD_STRING, gen, "nm", quickJsonName.Text);
            output("Set title | " + quickJsonName.Text);
        }

        #endregion

        private void jobCopyGetCurrent_Click(object sender, EventArgs e)
        {
            uint location = (uint)RPC.Call(Natives.NETWORK_GET_CONTENT_ID);
            jobCopyCurrentId.Text = PS3.Extension.ReadString(location);
            output("Get current job id | " + jobCopyCurrentId.Text);
        }

        private void objectGetType_Click(object sender, EventArgs e)
        {
            int type = RPC.Call(Natives._OBJECT_VALUE_GET_TYPE, (int)objectTypeAddress.Value, objectTypeKey.Text);
            switch(type)
            {
                case 1:
                    objectType.Text = "Bool";
                    break;
                case 2:
                    objectType.Text = "Integer";
                    break;
                case 3:
                    objectType.Text = "Float";
                    break;
                case 4:
                    objectType.Text = "String";
                    break;
                case 5:
                    objectType.Text = "Vector3";
                    break;
                case 6:
                    objectType.Text = "Object";
                    break;
                case 7:
                    objectType.Text = "Array";
                    break;
                default:
                    objectType.Text = "Unkown";
                    break;
            }
            output("Get type | " + objectTypeAddress.Value + " | " + objectTypeKey.Text + " | " + objectType.Text);
        }

        private void customServerCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (customServerCheckbox.Checked)
            {
                Network.switchServer("rosser");
                output("Switched to aaronrosser.xyz");
            } else
            {
                Network.switchServer("Rockstar");
                output("Switched to rockstargames.com");
            }
        }

        #region Jobs tab

        private void jsonFinderFind_Click(object sender, EventArgs e)
        {
            output(Network.getJsonFromImage(jsonFinderDirectURL.Text));
        }

        private void uploadAndLoadJsonFile_Click(object sender, EventArgs e)
        {
            Stream myStream = null;
            System.Windows.Forms.OpenFileDialog theDialog = new System.Windows.Forms.OpenFileDialog();
            theDialog.Title = "Open JSON File";
            theDialog.Filter = "JSON files|*.json;*.ugc|All files (*.*)|*.*";
            if (registyKeyGet("defaultJsonFileDialogLocation") != null) theDialog.InitialDirectory = registyKeyGet("defaultJsonFileDialogLocation").ToString();
            else theDialog.InitialDirectory = "~";

            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = theDialog.OpenFile()) != null)
                    {
                        using (StreamReader reader = new StreamReader(myStream))
                        {
                            string json = reader.ReadToEnd();
                            if (isValidJson(json))
                            {
                                string id = Network.uploadJsonToServer(json);
                                if (id != null)
                                {
                                    if (Network.loadJsonfileFromServer(id, true)) output("File loaded");
                                    else output("File failed to download");
                                } else
                                {
                                    output("Upload failed");
                                }
                            }
                        }
                    }
                    registryKeyAdd("defaultJsonFileDialogLocation", theDialog.FileName);
                }
                catch (Exception ex)
                {
                    output("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void jobsPublishedSaved_SelectedIndexChanged(object sender, EventArgs e)
        {
            List<string> jobNames = new List<string>();
            string name = menu.getJobName(0, (bool)jobsPublishedSaved.EditValue);
            int i = 0;

            while (name != "" && name != null)
            {
                name = menu.getJobName(i, (bool)jobsPublishedSaved.EditValue);
                if (name != "" && name != null) jobNames.Add(name);
                i++;
            }

            jobsListbox.DataSource = jobNames;

            if ((bool)jobsPublishedSaved.EditValue) jobsPublish.Enabled = true;
            else jobsPublish.Enabled = false;
        }

        private void jobsSetId_Click(object sender, EventArgs e)
        {
            if (jobsListbox.SelectedIndex != -1) menu.setJobId(jobsListbox.SelectedIndex, jobsId.Text, (bool)jobsPublishedSaved.EditValue);
        }

        private void jobsRefresh_Click(object sender, EventArgs e)
        {
            List<string> jobNames = new List<string>();
            string name = menu.getJobName(0, (bool)jobsPublishedSaved.EditValue);
            int i = 0;

            while (name != "" && name != null)
            {
                name = menu.getJobName(i, (bool)jobsPublishedSaved.EditValue);
                if (name != "" && name != null) jobNames.Add(name);
                i++;
            }

            jobsListbox.DataSource = jobNames;
        }

        private void jobsCopy_Click(object sender, EventArgs e)
        {
            output("Copy job | " + jobsId.Text + " | " + RPC.Call(Natives.NETWORK_COPY_0x08243B79, jobsId.Text, "gta5mission"));
        }

        private void jobsDelete_Click(object sender, EventArgs e)
        {
            if (DevExpress.XtraEditors.XtraMessageBox.Show("Are you sure you want to delete the job with the id " + jobsId.Text, "Delete job", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                output("Delete job | " + jobsId.Text + " | " + RPC.Call(Natives.NETWORK_DELETE_0x48CCC328_0xD7793B5B68EE55D5, jobsId.Text, 1, "gta5mission"));
            }
        }

        private void jobsSetDefaultImage_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog theDialog = new System.Windows.Forms.OpenFileDialog();
            theDialog.Title = "Open jpg File";
            theDialog.Filter = "JPG files|*.jpg|All files (*.*)|*.*";
            if (registyKeyGet("defaultPhotoFileDialogLocation") != null) theDialog.InitialDirectory = registyKeyGet("defaultPhotoFileDialogLocation").ToString();
            else theDialog.InitialDirectory = "~";

            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Image img = Image.FromFile(theDialog.FileName);

                    if (img.RawFormat.Equals(ImageFormat.Jpeg) && img.Width == 320 && img.Height == 180)
                    {

                    }

                    registryKeyAdd("defaultPhotoFileDialogLocation", theDialog.FileName);
                }
                catch (Exception ex)
                {
                    output("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void imageServerAdd_Click(object sender, EventArgs e)
        {
            string id = (menu.getJobId(jobsListbox.SelectedIndex, (bool)jobsPublishedSaved.EditValue));
            if (id != "" && id != null)
            {
                output(Network.addImageToServer(menu.getJobId(jobsListbox.SelectedIndex, (bool)jobsPublishedSaved.EditValue), imageServerImageUrl.Text));
            } else
            {
                output("Please select a job");
            }
        }

        private void jobsBookmark_Click(object sender, EventArgs e)
        {
            output("Bookmark job | " + jobsId.Text + " | " + RPC.Call(Natives.NETWORK_BOOKMARK_980D45D7, jobsId.Text, 1, "gta5mission"));
        }

        private void jobsListbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string id = menu.getJobId(jobsListbox.SelectedIndex, (bool)jobsPublishedSaved.EditValue);
            jobsId.Text = id;
            //Get url redirect for job image
            if (id != null && id != "")
            {
                string photoRedirectURL = Network.getImageRedirectURL(id);
                if (photoRedirectURL.Contains("http")) jobsCurrentImageRedirect.Text = photoRedirectURL;
                else jobsCurrentImageRedirect.Text = "";
            } else
            {
                jobsCurrentImageRedirect.Text = "";
            }
        }

        private void jobsPublish_Click(object sender, EventArgs e)
        {
            output("Publish job | " + jobsId.Text + " | " + RPC.Call(Natives.NETWORK_0x1DE0F5F50D723CAA_PUBLISH, jobsId.Text, "", "gta5mission"));
        }

        private void customMetaPushDatafileToMeta_Click(object sender, EventArgs e)
        {
            if (preload_find() != 0) output("Push datafile to meta | " + RPC.Call(Natives.DATAFILE_0x4E03F632_PushDataFileToMeta));
            else output("Datafile not found");
        }


        private void customMetaCreateJob_Click(object sender, EventArgs e)
        {
            output("Create job | " + RPC.Call(Natives.DATAFILE_UPDATE_JOB_0x4645DE9980999E93, customMetaName.Text, customMetaDescription.Text, customMetaSearchTags.Text, "gta5mission", true));
        }

        private void customMetaUpdateJob_Click(object sender, EventArgs e)
        {
            output("Update job | " + RPC.Call(Natives.DATAFILE_UPDATE_JOB_0x4645DE9980999E93, customMetaJobId.Text, customMetaName.Text , customMetaDescription.Text, customMetaSearchTags.Text, "gta5mission"));
        }
        #endregion

        #endregion

        private void outputBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void disableCaching_CheckedChanged(object sender, EventArgs e)
        {
            Network.disableCaching(disableCaching.Checked);
        }

        private void jobsCurrentImageRedirect_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(jobsCurrentImageRedirect.Text);
        }

        private void jobs2UploadAndLoadJson_Click(object sender, EventArgs e)
        {
            Stream myStream = null;
            System.Windows.Forms.OpenFileDialog theDialog = new System.Windows.Forms.OpenFileDialog();
            theDialog.Title = "Open JSON File";
            theDialog.Filter = "JSON files|*.json;*.ugc|All files (*.*)|*.*";
            if (registyKeyGet("defaultJsonFileDialogLocation") != null) theDialog.InitialDirectory = registyKeyGet("defaultJsonFileDialogLocation").ToString();
            else theDialog.InitialDirectory = "~";

            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = theDialog.OpenFile()) != null)
                    {
                        using (StreamReader reader = new StreamReader(myStream))
                        {
                            string json = reader.ReadToEnd();
                            if (isValidJson(json))
                            {
                                string id = Network.uploadJsonToServer(json);
                                if (id != null)
                                {
                                    if (Network.loadJsonfileFromServer(id, true)) output("File loaded");
                                    else output("File failed to download");
                                }
                                else
                                {
                                    output("Upload failed");
                                }
                            }
                        }
                    }
                    registryKeyAdd("defaultJsonFileDialogLocation", theDialog.FileName);
                }
                catch (Exception ex)
                {
                    output("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void jobs2DoEverything_Click(object sender, EventArgs e)
        {
	        var theDialog = new System.Windows.Forms.OpenFileDialog
            {
	            Title = "Open JSON File",
	            Filter = "JSON files|*.json;*.ugc|All files (*.*)|*.*",
	            InitialDirectory = registyKeyGet("defaultJsonFileDialogLocation") != null
		            ? registyKeyGet("defaultJsonFileDialogLocation").ToString()
		            : "~"
            };

            if (theDialog.ShowDialog() != DialogResult.OK) return;
            try
            {
	            Stream myStream = null;
	            if ((myStream = theDialog.OpenFile()) != null)
	            {
		            using (StreamReader reader = new StreamReader(myStream))
		            {
			            string json = reader.ReadToEnd();
			            if (!isValidJson(json))
			            {
				            return;
			            }
			            string id = Network.uploadJsonToServer(json);
			            if (id == null)
			            {
				            output("Upload failed");
				            return;
			            }

			            if (!Network.loadJsonfileFromServer(id, true))
			            {
				            output("File failed to download");
							return;
			            }
			            output("File loaded for meta");

			            //Check file is loaded
			            if (preload_find() == 0)
			            {
				            output("Datafile not found");
				            return;
			            }
			            //Push json to meta
			            output("Push datafile to meta | " + RPC.Call(Natives.DATAFILE_0x4E03F632_PushDataFileToMeta));
			            if (!Network.loadJsonfileFromServer(id, true))
			            {
				            output("File failed to download");
				            return;
			            }

			            output("File loaded for json");
			            //Overwrite job
			            output("Update job | " + RPC.Call(Natives.DATAFILE_UPDATE_JOB_0x4645DE9980999E93, customMetaJobId.Text, 
				                   customMetaName.Text, customMetaDescription.Text, customMetaSearchTags.Text, "gta5mission"));
			            if (DevExpress.XtraEditors.XtraMessageBox.Show(
				                "Open the job on PS4 and press save once.\nHave you saved the job once on PS4?",
				                "Save job", MessageBoxButtons.YesNo) != DialogResult.Yes)
			            {
				            output("Job not copied or overwritten with blank json");
				            return;
			            }
			            int copyResult = RPC.Call(Natives.NETWORK_COPY_0x08243B79,
					            customMetaJobId.Text, "gta5mission");

			            if (!Convert.ToBoolean(copyResult))
			            {
				            output("Job copy failed");
				            return;
			            }
			            //Copy job
			            output("Copy job | " + customMetaJobId.Text + " | " + copyResult);
			            if (!Network.loadJsonfileFromServer("default_ps3_job", true))
			            {
				            output("File failed to download");
				            return;
			            }
			            output("File loaded for meta");

			            //Check file is loaded
			            if (preload_find() == 0)
			            {
				            output("Datafile not found");
				            return;
			            }
			            //Push json to meta
			            output("Push datafile to meta | " +
			                   RPC.Call(Natives
				                   .DATAFILE_0x4E03F632_PushDataFileToMeta));
			            if (!Network.loadJsonfileFromServer("default_ps3_job", true))
			            {
				            output("File failed to download");
				            return;
			            }
			            output("File loaded for json");
				            //Overwrite job
				            output("Update job | " + RPC.Call(Natives.DATAFILE_UPDATE_JOB_0x4645DE9980999E93, customMetaJobId.Text, 
					                   "DON'T DELETE!!!!", customMetaDescription.Text, customMetaSearchTags.Text, "gta5mission"));
				            output("Job overwriteen copied and overwritten again");
		            }
	            }
	            registryKeyAdd("defaultJsonFileDialogLocation", theDialog.FileName);
            }
            catch (Exception ex)
            {
	            output("Error: Could not read file from disk. Original error: " + ex.Message);
            }
        }

        private void jobs2CopyAll_Click(object sender, EventArgs e)
        {
            List<string> failed = new List<string>();
            foreach (string line in jobs2CopyIds.Lines)
            {
                int result = RPC.Call(Natives.NETWORK_COPY_0x08243B79, line, "gta5mission");
                if (result == 0)
                {
                    failed.Add(line);
                }
                output("Copy job | " + line + " | " + result);
                Thread.Sleep(1400);
            }
            List<string> failedAgain = new List<string>();
            foreach (string line in failed)
            {
                int result = RPC.Call(Natives.NETWORK_COPY_0x08243B79, line, "gta5mission");
                if (result == 0)
                {
                    failedAgain.Add(line);
                }
                output("Copy job (retry) | " + line + " | " + result);
                Thread.Sleep(1600);
            }
            if (failedAgain.Count > 0)
            {
                output("Failed jobs | " + String.Join(", ", failedAgain.ToArray()));
            }
        }

        private void customMetaJobId_EditValueChanged(object sender, EventArgs e)
        {
            registryKeyAdd("jobId", customMetaJobId.Text);
        }
    }
}
