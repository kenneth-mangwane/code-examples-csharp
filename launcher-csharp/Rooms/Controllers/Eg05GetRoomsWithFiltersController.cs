﻿using System.Globalization;
using DocuSign.Rooms.Api;
using DocuSign.Rooms.Client;
using DocuSign.Rooms.Model;
using eg_03_csharp_auth_code_grant_core.Controllers;
using eg_03_csharp_auth_code_grant_core.Models;
using eg_03_csharp_auth_code_grant_core.Rooms.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace eg_03_csharp_auth_code_grant_core.Rooms.Controllers
{
    [Area("Rooms")]
    [Route("Eg05")]
    public class Eg05GetRoomsWithFiltersController : EgController
    {
        private readonly IRoomsApi _roomsApi;

        public Eg05GetRoomsWithFiltersController(
            DSConfiguration dsConfig,
            IRequestItemsService requestItemsService,
            IRoomsApi roomsApi) : base(dsConfig, requestItemsService)
        {
            _roomsApi = roomsApi;
        }

        public override string EgName => "Eg05";

        [BindProperty]
        public RoomFilterModel RoomFilterModel { get; set; }

        protected override void InitializeInternal()
        {
            RoomFilterModel = new RoomFilterModel();
        }

        [MustAuthenticate]
        [Route("ExportData")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExportData(RoomFilterModel roomFilterModel)
        {
            // Step 1. Obtain your OAuth token
            string accessToken = RequestItemsService.User.AccessToken; // Represents your {ACCESS_TOKEN}
            var basePath = $"{RequestItemsService.Session.RoomsApiBasePath}/restapi"; // Base API path

            // Step 2: Construct your API headers
            ConstructApiHeaders(accessToken, basePath);

            string accountId = RequestItemsService.Session.AccountId; // Represents your {ACCOUNT_ID}

            try
            {
                // Step 3: Call the Rooms API to get rooms with filters
                RoomSummaryList rooms = _roomsApi.GetRooms(accountId, new RoomsApi.GetRoomsOptions
                {
                    fieldDataChangedStartDate = roomFilterModel.FieldDataChangedStartDate.ToString(CultureInfo.InvariantCulture),
                    fieldDataChangedEndDate = roomFilterModel.FieldDataChangedEndDate.ToString(CultureInfo.InvariantCulture)
                });

                ViewBag.h1 = "The rooms with filters was loaded";
                ViewBag.message = $"Results from the Rooms: GetRooms method. FieldDataChangedStartDate: " +
                                  $"{ roomFilterModel.FieldDataChangedStartDate.Date.ToShortDateString() }, " +
                                  $"FieldDataChangedEndDate: { roomFilterModel.FieldDataChangedEndDate.Date.ToShortDateString() } :";
                ViewBag.Locals.Json = JsonConvert.SerializeObject(rooms, Formatting.Indented);

                return View("example_done");
            }
            catch (ApiException apiException)
            {
                ViewBag.errorCode = apiException.ErrorCode;
                ViewBag.errorMessage = apiException.Message;

                return View("Error");
            }
        }

        private void ConstructApiHeaders(string accessToken, string basePath)
        {
            var config = new Configuration(new ApiClient(basePath));
            config.AddDefaultHeader("Authorization", "Bearer " + accessToken);

            _roomsApi.Configuration = config;
        }
    }
}