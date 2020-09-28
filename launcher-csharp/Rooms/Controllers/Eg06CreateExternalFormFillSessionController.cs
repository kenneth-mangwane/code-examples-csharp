﻿using DocuSign.CodeExamples.Controllers;
using DocuSign.CodeExamples.Models;
using DocuSign.CodeExamples.Rooms.Models;
using DocuSign.Rooms.Api;
using DocuSign.Rooms.Client;
using DocuSign.Rooms.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DocuSign.CodeExamples.Rooms.Controllers
{
    [Area("Rooms")]
    [Route("Eg06")]
    public class Eg06CreateExternalFormFillSessionController : EgController
    {
        public Eg06CreateExternalFormFillSessionController(
            DSConfiguration dsConfig,
            IRequestItemsService requestItemsService) : base(dsConfig, requestItemsService)
        {
        }

        public override string EgName => "Eg06";

        [BindProperty]
        public RoomDocumentModel RoomDocumentModel { get; set; }

        protected override void InitializeInternal()
        {
            RoomDocumentModel = new RoomDocumentModel();
        }

        [MustAuthenticate]
        [HttpGet]
        public override IActionResult Get()
        {
            // Step 1. Obtain your OAuth token
            string accessToken = RequestItemsService.User.AccessToken; // Represents your {ACCESS_TOKEN}
            var basePath = $"{RequestItemsService.Session.RoomsApiBasePath}/restapi"; // Base API path

            // Step 2: Construct your API headers
            var roomsApi = new RoomsApi(new ApiClient(basePath));
            var formLibrariesApi = new FormLibrariesApi(new ApiClient(basePath));
            formLibrariesApi.ApiClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + accessToken);
            roomsApi.ApiClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + accessToken);

            string accountId = RequestItemsService.Session.AccountId; // Represents your {ACCOUNT_ID}

            try
            {
                //Step 3: Get Rooms 
                RoomSummaryList rooms = roomsApi.GetRooms(accountId);

                RoomDocumentModel = new RoomDocumentModel { Rooms = rooms.Rooms };

                return View("Eg06", this);
            }
            catch (ApiException apiException)
            {
                ViewBag.errorCode = apiException.ErrorCode;
                ViewBag.errorMessage = apiException.Message;

                return View("Error");
            }
        }

        [MustAuthenticate]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SelectRoom(RoomDocumentModel roomDocumentModel)
        {
            // Step 1. Obtain your OAuth token
            string accessToken = RequestItemsService.User.AccessToken; // Represents your {ACCESS_TOKEN}
            var basePath = $"{RequestItemsService.Session.RoomsApiBasePath}/restapi"; // Base API path

            // Step 2: Construct your API headers
            var roomsApi = new RoomsApi(new ApiClient(basePath));
            roomsApi.ApiClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + accessToken);

            string accountId = RequestItemsService.Session.AccountId; // Represents your {ACCOUNT_ID}

            try
            {
                //Step 3: Get Room Documents
                RoomDocumentList documents = roomsApi.GetDocuments(accountId, roomDocumentModel.RoomId);

                RoomDocumentModel.Documents = documents.Documents;

                return View("Eg06", this);
            }
            catch (ApiException apiException)
            {
                ViewBag.errorCode = apiException.ErrorCode;
                ViewBag.errorMessage = apiException.Message;

                return View("Error");
            }
        }

        [MustAuthenticate]
        [Route("ExportData")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExportData(RoomDocumentModel roomDocumentModel)
        {
            // Step 1. Obtain your OAuth token
            string accessToken = RequestItemsService.User.AccessToken; // Represents your {ACCESS_TOKEN}
            var basePath = $"{RequestItemsService.Session.RoomsApiBasePath}/restapi"; // Base API path

            // Step 2: Construct your API headers
            var roomsApi = new RoomsApi(new ApiClient(basePath));
            var externalFormFillSessionsApi = new ExternalFormFillSessionsApi(new ApiClient(basePath));
            externalFormFillSessionsApi.ApiClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + accessToken);
            roomsApi.ApiClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + accessToken);

            string accountId = RequestItemsService.Session.AccountId; // Represents your {ACCOUNT_ID}

            try
            {
                // Step 3: Call the Rooms API to create external form fill session
                ExternalFormFillSession url = externalFormFillSessionsApi.CreateExternalFormFillSession(
                    accountId,
                    new ExternalFormFillSessionForCreate(roomDocumentModel.DocumentId.ToString(), roomDocumentModel.RoomId));

                ViewBag.h1 = "External form fill sessions was successfully created";
                ViewBag.message = $"To fill the form, navigate following URL: <a href='{url.Url}' target='_blank'>Fill the form</a>";
                ViewBag.Locals.Json = JsonConvert.SerializeObject(url, Formatting.Indented);

                return View("example_done");
            }
            catch (ApiException apiException)
            {
                ViewBag.errorCode = apiException.ErrorCode;
                ViewBag.errorMessage = apiException.Message;

                return View("Error");
            }
        }
    }
}