﻿//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using System;
using System.Linq;
#if WINDOWS_UWP
using System.Runtime.InteropServices.WindowsRuntime;
#endif
using System.IO;

namespace SpeechToText
{
#if WINDOWS_UWP
    /// <summary>
    /// class SpeechToTextClient: SpeechToText UWP Client
    /// </summary>
    /// <info>
    /// Event data that describes how this page was reached.
    /// This parameter is typically used to configure the page.
    /// </info>
    public class SpeechToTextClient
    {
        private string SubscriptionKey;
        private string Token;
        private SpeechToTextMainStream STTStream;
        private const string SpeechAuthUrl = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
        private string AuthUrl = SpeechAuthUrl;
        private const string CustomSpeechAuthUrl = "https://westus.api.cognitive.microsoft.com/sts/v1.0/issueToken";
        private const string SpeechUrl = "https://{0}/speech/recognition/{1}/cognitiveservices/v1";

        private bool isRecordingInitialized;
        private bool isRecording;
        private ulong maxStreamSizeInBytes;
        private UInt16 thresholdDuration;
        private UInt16 thresholdLevel;
        private Windows.Media.Capture.MediaCapture mediaCapture;

        private string apiString = "interactive";
        private string hostnameString = "speech.platform.bing.com";
        /// <summary>
        /// class SpeechToTextClient constructor
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        public SpeechToTextClient()
        {
            SubscriptionKey = string.Empty;
            Token = string.Empty;
            isRecordingInitialized = false;
            isRecording = false;
            thresholdDuration = 0;
            thresholdLevel = 0;
            maxStreamSizeInBytes = 0;
        }
        /// <summary>
        /// SetAPI method
        /// </summary>
        /// <param name="APIstring">String associated with the endpoint url (interactive, conversation, dictation) 
        /// 
        /// </param>
        /// <return>True if successfull 
        /// </return>
        public bool SetAPI(string HostnameString, string APIstring)
        {
            bool result = false;
            if (string.Equals(APIstring, "interactive") ||
                string.Equals(APIstring, "conversation") ||
                string.Equals(APIstring, "dictation"))
            {
                apiString = APIstring;
                if (!string.IsNullOrEmpty(HostnameString))
                {
                    hostnameString = HostnameString;
                    if (string.Equals(hostnameString, "speech.platform.bing.com"))
                        AuthUrl = SpeechAuthUrl;
                    else
                        AuthUrl = CustomSpeechAuthUrl;
                }
                result = true;
            }
            return result;
        }
        /// <summary>
        /// ClearToken method
        /// </summary>
        /// <return>true.
        /// </return>
        public bool ClearToken()
        {
            Token = String.Empty;
            return true;
        }

        /// <summary>
        /// GetToken method
        /// </summary>
        /// <param name="subscriptionKey">SubscriptionKey associated with the SpeechToText 
        /// Cognitive Service subscription.
        /// </param>
        /// <return>Token which is used for all calls to the SpeechToText REST API.
        /// </return>
        public async System.Threading.Tasks.Task<string> GetToken(string subscriptionKey )
        {
            if (string.IsNullOrEmpty(subscriptionKey))
                return string.Empty;
            SubscriptionKey = subscriptionKey;
            try
            {
                Token = string.Empty;
                Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();
                hc.DefaultRequestHeaders.TryAppendWithoutValidation("Ocp-Apim-Subscription-Key", SubscriptionKey);
                Windows.Web.Http.HttpStringContent content = new Windows.Web.Http.HttpStringContent(String.Empty);

                Windows.Web.Http.HttpResponseMessage hrm = await hc.PostAsync(new Uri(AuthUrl), content);
                if (hrm != null)
                {
                    switch (hrm.StatusCode)
                    {
                        case Windows.Web.Http.HttpStatusCode.Ok:
                            var b = await hrm.Content.ReadAsBufferAsync();
                            string result = System.Text.UTF8Encoding.UTF8.GetString(b.ToArray());
                            if (!string.IsNullOrEmpty(result))
                            {
                                Token = "Bearer  " + result;
                                return Token;
                            }
                            break;

                        default:
                            System.Diagnostics.Debug.WriteLine("Http Response Error:" + hrm.StatusCode.ToString() + " reason: " + hrm.ReasonPhrase.ToString());
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while getting the token: " + ex.Message);
            }
            return string.Empty;
        }
        /// <summary>
        /// RenewToken method
        /// </summary>
        /// <param>
        /// </param>
        /// <return>Token which is used to all the calls to the SpeechToText REST API.
        /// </return>
        public async System.Threading.Tasks.Task<string> RenewToken()
        {
            if (string.IsNullOrEmpty(SubscriptionKey))
                return string.Empty;
            try
            {
                Token = string.Empty;
                Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();
                hc.DefaultRequestHeaders.TryAppendWithoutValidation("Ocp-Apim-Subscription-Key", SubscriptionKey);
                Windows.Web.Http.HttpStringContent content = new Windows.Web.Http.HttpStringContent(String.Empty);
                Windows.Web.Http.HttpResponseMessage hrm = await hc.PostAsync(new Uri(AuthUrl), content);
                if (hrm != null)
                {
                    switch (hrm.StatusCode)
                    {
                        case Windows.Web.Http.HttpStatusCode.Ok:
                            var b = await hrm.Content.ReadAsBufferAsync();
                            string result = System.Text.UTF8Encoding.UTF8.GetString(b.ToArray());
                            if (!string.IsNullOrEmpty(result))
                            {
                                Token = "Bearer  " + result;
                                return Token;
                            }
                            break;

                        default:
                            System.Diagnostics.Debug.WriteLine("Http Response Error:" + hrm.StatusCode.ToString() + " reason: " + hrm.ReasonPhrase.ToString());
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while getting the token: " + ex.Message);
            }
            return string.Empty;
        }
        /// <summary>
        /// HasToken method
        /// </summary>
        /// <param>Check if a Token has been acquired
        /// </param>
        /// <return>true if a Token has been acquired to use the SpeechToText REST API.
        /// </return>
        public bool HasToken()
        {
            if (string.IsNullOrEmpty(Token))
                return false;
            return true;
        }
        /// <summary>
        /// GetAudioStream method
        /// This method return the audio buffer (stream) which has been acquired while the client is continuously recording the audio.
        /// </summary>
        /// <param>
        /// </param>
        /// <return>The SpeechToTextAudioStream in the queue, null if the queue is empty.
        /// </return>
        public SpeechToTextAudioStream GetAudioStream()
        {
            return STTStream.GetAudioStream();
        }
        /// <summary>
        /// SendBuffer method
        /// This method sends the current audio buffer towards Cognitive Services REST API
        /// </summary>
        /// <param name="locale">language associated with the current buffer/recording.
        /// for instance en-US, fr-FR, pt-BR, ...
        /// </param>
        /// <return>The result of the SpeechToText REST API.
        /// </return>
        public async System.Threading.Tasks.Task<SpeechToTextResponse> SendBuffer(string locale, string resulttype)
        {
            SpeechToTextResponse r = null;
            int loop = 1;

            while (loop-- > 0)
            {
                try
                {
                    string speechUrl = string.Format(SpeechUrl, hostnameString, apiString) + "?language=" + locale + "&format=" + resulttype;
                    Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();
                    System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Authorization", Token);
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("ContentType", "audio/wav; codec=\"audio/pcm\"; samplerate=16000");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Accept", "application/json;text/xml");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Transfer-Encoding", "chunked");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Expect", "100-continue");

                    Windows.Web.Http.HttpResponseMessage hrm = null;
                    Windows.Web.Http.HttpStreamContent content = null;
                    if (STTStream != null)
                    {
                        content = new Windows.Web.Http.HttpStreamContent(STTStream.AsStream().AsInputStream());
                        //content.Headers.ContentLength = STTStream.GetLength();
                        //System.Diagnostics.Debug.WriteLine("REST API Post Content Length: " + content.Headers.ContentLength.ToString());
                        content.Headers.TryAppendWithoutValidation("ContentType", "audio/wav; codec=\"audio/pcm\"; samplerate=16000");
                        IProgress<Windows.Web.Http.HttpProgress> progress = new Progress<Windows.Web.Http.HttpProgress>(ProgressHandler);
                        hrm = await hc.PostAsync(new Uri(speechUrl), content).AsTask(cts.Token, progress);
                    }
                    if (hrm != null)
                    {
                        switch (hrm.StatusCode)
                        {
                            case Windows.Web.Http.HttpStatusCode.Ok:
                                var b = await hrm.Content.ReadAsBufferAsync();
                                string result = System.Text.UTF8Encoding.UTF8.GetString(b.ToArray());
                                if (!string.IsNullOrEmpty(result))
                                    r = new SpeechToTextResponse(result);
                                break;

                            case Windows.Web.Http.HttpStatusCode.Forbidden:
                                string token = await RenewToken();
                                if (string.IsNullOrEmpty(token))
                                {
                                    loop++;
                                }
                                break;

                            default:
                                int code = (int)hrm.StatusCode;
                                string HttpError = "Http Response Error: " + code.ToString() + " reason: " + hrm.ReasonPhrase.ToString();
                                System.Diagnostics.Debug.WriteLine(HttpError);
                                r = new SpeechToTextResponse(string.Empty, HttpError);
                                break;
                        }
                    }
                }
                catch (System.Threading.Tasks.TaskCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine("http POST canceled");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("http POST exception: " + ex.Message);
                }
                finally
                {
                    System.Diagnostics.Debug.WriteLine("http POST done");
                }
            }
            return r;
        }
        /// <summary>
        /// SendAudioStream method
        /// This method sends a SpeechToTextAudioStream towards Cognitive Services REST API.
        /// Usually, the method GetAudioStream returns the SpeechToTextAudioStream (if available) then the method SendAudioStream 
        /// sends a SpeechToTextAudioStream towards Cognitive Services REST API.
        /// </summary>
        /// <param name="locale">language associated with the current buffer/recording.
        /// for instance en-US, fr-FR, pt-BR, ...
        /// </param>
        /// <param name="stream">AudioStream which will be forwarded to REST API.
        /// </param>
        /// <return>The result of the SpeechToText REST API.
        /// </return>
        public async System.Threading.Tasks.Task<SpeechToTextResponse> SendAudioStream(string locale, string resulttype, SpeechToTextAudioStream stream)
        {
            SpeechToTextResponse r = null;
            int loop = 1;

            while (loop-- > 0)
            {
                try
                {
                    string speechUrl = string.Format(SpeechUrl, hostnameString, apiString) + "?language=" + locale + "&format=" + resulttype;

                    Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();
                    System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Authorization", Token);
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("ContentType", "audio/wav; codec=\"audio/pcm\"; samplerate=16000");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Accept", "application/json;text/xml");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Transfer-Encoding", "chunked");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Expect", "100-continue");

                    Windows.Web.Http.HttpResponseMessage hrm = null;
                    Windows.Web.Http.HttpStreamContent content = null;
                    content = new Windows.Web.Http.HttpStreamContent(stream.GetInputStreamAt(0));
                    //content.Headers.ContentLength = (ulong)stream.Size;
                    if ((content != null) && (stream.Size > 0))
                    {
                        System.Diagnostics.Debug.WriteLine("REST API Post Content Length: " + content.Headers.ContentLength.ToString());
                        content.Headers.TryAppendWithoutValidation("ContentType", "audio/wav; codec=\"audio/pcm\"; samplerate=16000");
                        IProgress<Windows.Web.Http.HttpProgress> progress = new Progress<Windows.Web.Http.HttpProgress>(ProgressHandler);
                        hrm = await hc.PostAsync(new Uri(speechUrl), content).AsTask(cts.Token, progress);
                    }
                    if (hrm != null)
                    {
                        switch (hrm.StatusCode)
                        {
                            case Windows.Web.Http.HttpStatusCode.Ok:
                                var b = await hrm.Content.ReadAsBufferAsync();
                                string result = System.Text.UTF8Encoding.UTF8.GetString(b.ToArray());
                                if (!string.IsNullOrEmpty(result))
                                    r = new SpeechToTextResponse(result);
                                break;
                            case Windows.Web.Http.HttpStatusCode.Forbidden:
                                string token = await RenewToken();
                                if (string.IsNullOrEmpty(token))
                                {
                                    loop++;
                                }
                                break;

                            default:
                                int code = (int)hrm.StatusCode;
                                string HttpError = "Http Response Error: " + code.ToString() + " reason: " + hrm.ReasonPhrase.ToString();
                                System.Diagnostics.Debug.WriteLine(HttpError);
                                r = new SpeechToTextResponse(string.Empty, HttpError);
                                break;
                        }
                    }
                }
                catch (System.Threading.Tasks.TaskCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine("http POST canceled");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("http POST exception: " + ex.Message);
                }
                finally
                {
                    System.Diagnostics.Debug.WriteLine("http POST done");
                }
            }
            return r;
        }
        /// <summary>
        /// SendStorageFile method
        /// </summary>
        /// <param name="wavFile">StorageFile associated with the audio file which 
        /// will be sent to the SpeechToText Services.
        /// </param>
        /// <param name="locale">language associated with the current buffer/recording.
        /// for instance en-US, fr-FR, pt-BR, ...
        /// </param>
        /// <return>The result of the SpeechToText REST API.
        /// </return>
        public async System.Threading.Tasks.Task<SpeechToTextResponse> SendStorageFile(Windows.Storage.StorageFile wavFile, string locale,string resulttype)
        {
            SpeechToTextResponse r = null;
            int loop = 1;

            while (loop-- > 0)
            {
                try
                {
                    string speechUrl = string.Format(SpeechUrl, hostnameString, apiString) + "?language=" + locale + "&format=" + resulttype;
                    Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();

                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Authorization", Token);
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("ContentType", "audio/wav; codec=\"audio/pcm\"; samplerate=16000");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Accept", "application/json;text/xml");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Transfer-Encoding", "chunked");
                    hc.DefaultRequestHeaders.TryAppendWithoutValidation("Expect", "100-continue");
                    Windows.Web.Http.HttpResponseMessage hrm = null;

                    Windows.Storage.StorageFile file = wavFile;
                    if (file != null)
                    {
                        using (var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                        {
                            if (STTStream != null)
                            {
                                STTStream.AudioLevel -= STTStream_AudioLevel;
                                STTStream.Dispose();
                                STTStream = null;
                            }
                            STTStream = SpeechToTextMainStream.Create();
                            if (STTStream != null)
                            {
                                byte[] byteArray = new byte[fileStream.Size];
                                fileStream.ReadAsync(byteArray.AsBuffer(), (uint)fileStream.Size, Windows.Storage.Streams.InputStreamOptions.Partial).AsTask().Wait();
                                STTStream.WriteAsync(byteArray.AsBuffer()).AsTask().Wait();

                                Windows.Web.Http.HttpStreamContent content = new Windows.Web.Http.HttpStreamContent(STTStream.AsStream().AsInputStream());
                //                content.Headers.ContentLength = STTStream.GetLength();
                  //              System.Diagnostics.Debug.WriteLine("REST API Post Content Length: " + content.Headers.ContentLength.ToString() + " bytes");
                                System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
                                IProgress<Windows.Web.Http.HttpProgress> progress = new Progress<Windows.Web.Http.HttpProgress>(ProgressHandler);
                                hrm = await hc.PostAsync(new Uri(speechUrl), content).AsTask(cts.Token, progress);
                                
                            }
                        }
                    }
                    if (hrm != null)
                    {
                        switch (hrm.StatusCode)
                        {
                            case Windows.Web.Http.HttpStatusCode.Ok:
                                var b = await hrm.Content.ReadAsBufferAsync();
                                string result = System.Text.UTF8Encoding.UTF8.GetString(b.ToArray());
                                if (!string.IsNullOrEmpty(result))
                                    r = new SpeechToTextResponse(result);
                                break;

                            case Windows.Web.Http.HttpStatusCode.Forbidden:
                                string token = await RenewToken();
                                if (string.IsNullOrEmpty(token))
                                {
                                    loop++;
                                }
                                break;

                            default:
                                int code = (int)hrm.StatusCode;
                                string HttpError = "Http Response Error: " + code.ToString() + " reason: " + hrm.ReasonPhrase.ToString();
                                System.Diagnostics.Debug.WriteLine(HttpError);
                                r = new SpeechToTextResponse(string.Empty, HttpError);
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception while sending the audio file:" + ex.Message);
                }
            }
            return r;
        }
        /// <summary>
        /// SaveBuffer method
        /// </summary>
        /// <param name="wavFile">StorageFile where the audio buffer 
        /// will be stored.
        /// </param>
        /// <param name="start">the position in the buffer of the first byte to save in a file. 
        /// by default the value is 0.
        /// </param>
        /// <param name="end">the position in the buffer of the last byte to save in a file
        /// by default the value is 0, if the value is 0 the whole buffer will be stored in a a file
        /// </param>
        /// <return>true if successful.
        /// </return>
        public async System.Threading.Tasks.Task<bool> SaveBuffer(Windows.Storage.StorageFile wavFile, UInt64 start = 0, UInt64 end = 0)
        {
            bool bResult = false;
            if (wavFile != null)
            {
                try
                {
                    using (Stream stream = await wavFile.OpenStreamForWriteAsync())
                    {
                        if ((stream != null) && (STTStream != null))
                        {
                            stream.SetLength(0);
                            if ((start == 0) && (end == 0))
                            {
                                await STTStream.AsStream().CopyToAsync(stream);
                                System.Diagnostics.Debug.WriteLine("Audio Stream stored in: " + wavFile.Path);
                                bResult = true;
                            }
                            else if ((start >= 0) && (end > start))
                            {
                                var headerBuffer = STTStream.CreateWAVHeaderBuffer((uint)(end - start));
                                if (headerBuffer != null)
                                {
                                    byte[] buffer = new byte[headerBuffer.Length + (uint)(end - start)];
                                    if (buffer != null)
                                    {
                                        headerBuffer.CopyTo(buffer, headerBuffer.Length);
                                        ulong pos = STTStream.Position;
                                        STTStream.Seek(start);
                                        STTStream.ReadAsync(buffer.AsBuffer((int)headerBuffer.Length, (int)(end - start)), (uint)(end - start), Windows.Storage.Streams.InputStreamOptions.None).AsTask().Wait();
                                        STTStream.Seek(pos);
                                        MemoryStream bufferStream = new MemoryStream(buffer);
                                        await bufferStream.CopyToAsync(stream);
                                        System.Diagnostics.Debug.WriteLine("Audio Stream stored in: " + wavFile.Path);
                                        bResult = true;
                                    }
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception while saving the Audio Stream stored in: " + wavFile.Path + " Exception: " + ex.Message);

                }
            }
            return bResult;
        }
        /// <summary>
        /// IsRecording method
        /// </summary>
        /// <return>Return true if the Client is currently recording
        /// </return>
        public bool IsRecording()
        {
            return isRecording;
        }
        /// <summary>
        /// GetBufferLength method
        /// Return the length of the current audio buffer
        /// </summary>
        /// <return>the length of the WAV buffer in ulong.
        /// </return>
        public ulong GetBufferLength()
        {
            if (STTStream != null)
            {
                return STTStream.GetLength();
            }
            return 0;
        }
        /// <summary>
        /// StartRecording method
        /// Start to record audio using the microphone.
        /// The audio stream in stored in memory with no limit of size.
        /// </summary>
        /// <param name="MaxStreamSizeInBytes">
        /// This parameter defines the max size of the buffer in memory. When the size of the buffer is over this limit, the 
        /// client create another stream and remove the previouw stream. 
        /// By default the value is 0, in that case the audio stream in stored in memory with no limit of size.
        /// </param>
        /// <return>return true if successful.
        /// </return>
        public async System.Threading.Tasks.Task<bool> StartRecording(ulong MaxStreamSizeInBytes = 0)
        {
            return await StartContinuousRecording(MaxStreamSizeInBytes,0,0);
        }
        /// <summary>
        /// StartRecording method
        /// Start to record audio using the microphone.
        /// The audio stream in stored in memory with no limit of size.
        /// </summary>
        /// <param name="MaxStreamSizeInBytes">
        /// This parameter defines the max size of the buffer in memory. When the size of the buffer is over this limit, the 
        /// client create another stream and remove the previouw stream. 
        /// By default the value is 0, in that case the audio stream in stored in memory with no limit of size.
        /// </param>
        /// <param name="ThresholdDuration">
        /// The duration in milliseconds for the calculation of the average audio level. 
        /// With this parameter you define the period during which the average level is measured. 
        /// If the value is 0, no buffer will be sent to Cognitive Services.
        /// </param>
        /// <param name="ThresholdLevel">
        /// The minimum audio level average necessary to trigger the recording, 
        /// it's a value between 0 and 65535. You can tune this value after several microphone tests.
        /// If the value is 0, no buffer will be sent to Cognitive Services.
        /// </param>
        /// <return>return true if successful.
        /// </return>
        public async System.Threading.Tasks.Task<bool> StartContinuousRecording(ulong MaxStreamSizeInBytes, UInt16 ThresholdDuration, UInt16 ThresholdLevel)
        {
            thresholdDuration = ThresholdDuration;
            thresholdLevel = ThresholdLevel;
            bool bResult = false;
            maxStreamSizeInBytes = MaxStreamSizeInBytes;
            if (isRecordingInitialized != true)
                await InitializeRecording();
            if(STTStream != null)
            {
                STTStream.BufferReady -= STTStream_BufferReady;
                STTStream.AudioLevel -= STTStream_AudioLevel;
                STTStream.Dispose();
                STTStream = null;
            }
            STTStream = SpeechToTextMainStream.Create(maxStreamSizeInBytes, thresholdDuration, thresholdLevel);
            STTStream.AudioLevel += STTStream_AudioLevel;
            STTStream.BufferReady += STTStream_BufferReady;

            if ((STTStream != null) && (isRecordingInitialized == true))
            {
                try
                {
                    Windows.Media.MediaProperties.MediaEncodingProfile MEP = Windows.Media.MediaProperties.MediaEncodingProfile.CreateWav(Windows.Media.MediaProperties.AudioEncodingQuality.Auto);
                    if (MEP != null)
                    {
                        if (MEP.Audio != null)
                        {
                            uint framerate = 16000;
                            uint bitsPerSample = 16;
                            uint numChannels = 1;
                            uint bytespersecond = 32000;
                            MEP.Audio.Properties[WAVAttributes.MF_MT_AUDIO_SAMPLES_PER_SECOND] = framerate;
                            MEP.Audio.Properties[WAVAttributes.MF_MT_AUDIO_NUM_CHANNELS] = numChannels;
                            MEP.Audio.Properties[WAVAttributes.MF_MT_AUDIO_BITS_PER_SAMPLE] = bitsPerSample;
                            MEP.Audio.Properties[WAVAttributes.MF_MT_AUDIO_AVG_BYTES_PER_SECOND] = bytespersecond;
                            foreach (var Property in MEP.Audio.Properties)
                            {
                                System.Diagnostics.Debug.WriteLine("Property: " + Property.Key.ToString());
                                System.Diagnostics.Debug.WriteLine("Value: " + Property.Value.ToString());
                                if (Property.Key == new Guid("5faeeae7-0290-4c31-9e8a-c534f68d9dba"))
                                    framerate = (uint)Property.Value;
                                if (Property.Key == new Guid("f2deb57f-40fa-4764-aa33-ed4f2d1ff669"))
                                    bitsPerSample = (uint)Property.Value;
                                if (Property.Key == new Guid("37e48bf5-645e-4c5b-89de-ada9e29b696a"))
                                    numChannels = (uint)Property.Value;

                            }
                        }
                        if (MEP.Container != null)
                        {
                            foreach (var Property in MEP.Container.Properties)
                            {
                                System.Diagnostics.Debug.WriteLine("Property: " + Property.Key.ToString());
                                System.Diagnostics.Debug.WriteLine("Value: " + Property.Value.ToString());
                            }
                        }
                    }
                    await mediaCapture.StartRecordToStreamAsync(MEP, STTStream);
                    bResult = true;
                    isRecording = true;
                    System.Diagnostics.Debug.WriteLine("Recording in audio stream...");
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Exception while recording in audio stream:" + e.Message);
                }
            }
            return bResult;
        }


        /// <summary>
        /// StopRecording method
        /// </summary>
        /// <param>
        /// Stop to record audio .
        /// The audio stream is still in stored in memory
        /// </param>
        /// <return>return true if successful.
        /// </return>
        public async System.Threading.Tasks.Task<bool> StopRecording()
        {
            // Stop recording and dispose resources
            if (mediaCapture != null)
            {
                if (isRecording == true)
                {
                    await mediaCapture.StopRecordAsync();
                    isRecording = false;
                }
            }
            return true;
        }

        /// <summary>
        /// Cleans up the microphone resources and the stream and unregisters from MediaCapture events
        /// </summary>
        /// <returns>true if successful</returns>
        public async System.Threading.Tasks.Task<bool> CleanupRecording()
        {
            if (isRecordingInitialized)
            {
                // If a recording is in progress during cleanup, stop it to save the recording
                if (isRecording)
                {
                    await StopRecording();
                }
                isRecordingInitialized = false;
            }

            if (mediaCapture != null)
            {
                mediaCapture.RecordLimitationExceeded -= mediaCapture_RecordLimitationExceeded;
                mediaCapture.Failed -= mediaCapture_Failed;
                mediaCapture.Dispose();
                mediaCapture = null;
            }
            if (STTStream != null)
            {
                STTStream.BufferReady -= STTStream_BufferReady;
                STTStream.AudioLevel -= STTStream_AudioLevel;
                STTStream.Dispose();
                STTStream = null;
            }
            return true;
        }
        /// <summary>
        /// Event which returns the position of the buffer ready to be sent 
        /// This event is fired with continuous recording
        /// </summary>
        public delegate void BufferReadyEventHandler(object sender);
        public event BufferReadyEventHandler BufferReady;
        
        /// <summary>
        /// Event which returns the Audio Level of the audio samples
        /// being stored in the audio buffer
        /// </summary>
        public delegate void AudioLevelEventHandler(object sender, double level);
        public event AudioLevelEventHandler AudioLevel;

        /// <summary>
        /// Event which returns the Audio Capture Errors while 
        /// a recording is in progress
        /// </summary>
        /// <returns>true if successful</returns>
        public delegate void AudioCaptureErrorEventHandler(object sender, string message);
        public event AudioCaptureErrorEventHandler AudioCaptureError;
#region private
        private async System.Threading.Tasks.Task<bool> InitializeRecording()
        {
            isRecordingInitialized = false;
            try
            {
                // Initialize MediaCapture
                mediaCapture = new Windows.Media.Capture.MediaCapture();

                await mediaCapture.InitializeAsync(new Windows.Media.Capture.MediaCaptureInitializationSettings
                {
                    //VideoSource = screenCapture.VideoSource,
                    //      AudioSource = screenCapture.AudioSource,
                    StreamingCaptureMode = Windows.Media.Capture.StreamingCaptureMode.Audio,
                    MediaCategory = Windows.Media.Capture.MediaCategory.Other,
                    AudioProcessing = Windows.Media.AudioProcessing.Raw

                });
                mediaCapture.RecordLimitationExceeded += mediaCapture_RecordLimitationExceeded;
                mediaCapture.Failed += mediaCapture_Failed;
                System.Diagnostics.Debug.WriteLine("Device Initialized Successfully...");
                isRecordingInitialized = true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception while initializing the device: " + e.Message);
            }
            return isRecordingInitialized;
        }
        async void mediaCapture_Failed(Windows.Media.Capture.MediaCapture sender, Windows.Media.Capture.MediaCaptureFailedEventArgs errorEventArgs)
        {
            System.Diagnostics.Debug.WriteLine("Fatal Error " + errorEventArgs.Message);
            await StopRecording();
            if (AudioCaptureError != null)
                AudioCaptureError(this, errorEventArgs.Message);
        }

        async void mediaCapture_RecordLimitationExceeded(Windows.Media.Capture.MediaCapture sender)
        {
            System.Diagnostics.Debug.WriteLine("Stopping Record on exceeding max record duration");
            await StopRecording();
            if (AudioCaptureError != null)
                AudioCaptureError(this, "Error Media Capture: Record Limitation Exceeded");
        }
        private  void STTStream_AudioLevel(object sender, double level)
        {
            //System.Diagnostics.Debug.WriteLine("STTStream_AmplitudeReading")
            if (AudioLevel != null)
                AudioLevel(sender, level);
        }
        private void STTStream_BufferReady(object sender)
        {
            if (BufferReady != null)
                BufferReady(sender);
        }


        private void ProgressHandler(Windows.Web.Http.HttpProgress progress)
        {
            System.Diagnostics.Debug.WriteLine("Http progress: " + progress.Stage.ToString() + " " + progress.BytesSent.ToString() + "/" + progress.TotalBytesToSend.ToString());
        }
#endregion private
    }
#endif
}
