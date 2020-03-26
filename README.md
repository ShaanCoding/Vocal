# Vocal

This application is a basic lightweight Youtube music streaming bot created in C# with the assistance of FFMPEG. This explores advanced image manipulation, advance API calls require OAuth 2.0, multi-threading, memory and more.

## Prerequisites
The running of this program requires several libaries these include:
* Google.Apis.Auth
* Google.Apis.Core
* Google.Apis.Youtube.v3
* Newtonsoft.Json
* TagLIb-Sharp

## How to use
To run this application you first must obtain a API key json file from the Google developer platform.
To do this you first must:
1. Create a project
2. Activate Youtube.Data.v3 on the project
3. Create an OAuth Key
4. From there you will be able to download a json file, place it in Vocal as "client_secret.json"

From there you will be able to just run the application as normal, it will request for you to authenticate and after signing into your google account it will then automatically begin streaming onto your account.

## Authors
* **Shaan Khan** - *All Work*

## License
This project is licensed under the Mozilla Public License 2.0 - see the [LICENSE](https://github.com/ShaanCoding/Vocal/blob/master/LICENSE) files for details
