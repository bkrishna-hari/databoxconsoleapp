using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure;
using Microsoft.Azure.Management.DataBox;
using Microsoft.Azure.Management.DataBox.Models;
using Microsoft.Rest.Azure;
using Microsoft.Rest.Azure.Authentication;

namespace DataboxConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Azure Databox Operations:");
            Console.WriteLine("  1 - Get job");
            Console.WriteLine("  2 - List jobs");
            Console.WriteLine("  3 - List jobs by resource group");
            Console.WriteLine("  4 - Validate shipping address");
            Console.WriteLine("  5 - Create job");
            Console.WriteLine("  6 - Cancel job");
            Console.WriteLine("  7 - Delete job");
            Console.WriteLine("  8 - Download shipping label uri");
            Console.WriteLine("  9 - Book shipment pickup");
            Console.WriteLine(" 10 - Get copy logs uri");
            Console.WriteLine(" 11 - Get secrets");
            Console.Write("\nChoose an option (1 to 11): ");

            string action = Console.ReadLine();

            switch (action)
            {
                case "1":
                    GetJob();
                    break;

                case "2":
                    ListJobs();
                    break;

                case "3":
                    ListJobsByResourceGroup();
                    break;

                case "4":
                    ValidateShippingAddress();
                    break;

                case "5":
                    CreateJob();
                    break;

                case "6":
                    CancelJob();
                    break;

                case "7":
                    DeleteJob();
                    break;

                case "8":
                    GetShippingLableUri();
                    break;

                case "9":
                    BookShipmentPickup();
                    break;

                case "10":
                    GetCopyLogsUri();
                    break;

                case "11":
                    GetSecrets();
                    break;

                default:
                    Console.WriteLine("Invalid option selected.");
                    break;
            }

            Console.Write("\nPress any key to exit.");
            Console.Read();
        }

        private static string tenantId;
        private static string subscriptionId;
        private static string aadApplicationId;
        private static string aadApplicationKey;

        /// <summary>
        /// Initializes a new instance of the DataBoxManagementClient class
        /// </summary>
        /// <returns></returns>
        static DataBoxManagementClient InitializeDataBoxClient()
        {
            const string frontDoorUrl = "https://login.microsoftonline.com";
            const string tokenUrl = "https://management.azure.com";

            // Fetch the configuration parameters.
            tenantId = CloudConfigurationManager.GetSetting("TenantId");
            subscriptionId = CloudConfigurationManager.GetSetting("SubscriptionId");
            aadApplicationId = CloudConfigurationManager.GetSetting("AADApplicationId");
            aadApplicationKey = CloudConfigurationManager.GetSetting("AADApplicationKey");

            // Validates AAD ApplicationId and returns token
            var credentials = ApplicationTokenProvider.LoginSilentAsync(
                                tenantId,
                                aadApplicationId,
                                aadApplicationKey,
                                new ActiveDirectoryServiceSettings()
                                {
                                    AuthenticationEndpoint = new Uri(frontDoorUrl),
                                    TokenAudience = new Uri(tokenUrl),
                                    ValidateAuthority = true,
                                }).GetAwaiter().GetResult();

            // Initializes a new instance of the DataBoxManagementClient class.
            DataBoxManagementClient dataBoxManagementClient = new DataBoxManagementClient(credentials);

            // Set SubscriptionId
            dataBoxManagementClient.SubscriptionId = subscriptionId;

            return dataBoxManagementClient;
        }

        /// <summary>
        /// Gets information about the specified job.
        /// </summary>
        private static void GetJob()
        {
            string resourceGroupName = "<resource-group-name>";
            string jobName = "<job-name>";
            string expand = "details";

            //Initializes a new instance of the DataBoxManagementClient class
            DataBoxManagementClient dataBoxManagementClient = InitializeDataBoxClient();

            // Gets information about the specified job.
            JobResource jobResource = JobsOperationsExtensions.Get(dataBoxManagementClient.Jobs, resourceGroupName, jobName, expand);
        }

        /// <summary>
        /// Lists all the jobs available under the subscription.
        /// </summary>
        private static void ListJobs()
        {
            //Initializes a new instance of the DataBoxManagementClient class
            DataBoxManagementClient dataBoxManagementClient = InitializeDataBoxClient();

            IPage<JobResource> jobPageList = null;
            List<JobResource> jobList = new List<JobResource>();

            do
            {
                // Lists all the jobs available under the subscription.
                if (jobPageList == null)
                {
                    jobPageList = JobsOperationsExtensions.List(dataBoxManagementClient.Jobs);
                }
                else
                {
                    jobPageList = JobsOperationsExtensions.ListNext(dataBoxManagementClient.Jobs, jobPageList.NextPageLink);
                }

                jobList.AddRange(jobPageList.ToList());

            } while (!(string.IsNullOrEmpty(jobPageList.NextPageLink)));
        }

        /// <summary>
        /// Lists all the jobs available under the given resource group.
        /// </summary>
        private static void ListJobsByResourceGroup()
        {
            //Initializes a new instance of the DataBoxManagementClient class
            DataBoxManagementClient dataBoxManagementClient = InitializeDataBoxClient();

            IPage<JobResource> jobPageList = null;
            List<JobResource> jobList = new List<JobResource>();
            string resourceGroupName = "<resource-group-name>";

            do
            {
                // Lists all the jobs available under resource group.
                if (jobPageList == null)
                {
                    jobPageList = JobsOperationsExtensions.ListByResourceGroup(dataBoxManagementClient.Jobs, resourceGroupName);
                }
                else
                {
                    jobPageList = JobsOperationsExtensions.ListByResourceGroupNext(dataBoxManagementClient.Jobs, jobPageList.NextPageLink);
                }

                jobList.AddRange(jobPageList.ToList());

            } while (!(string.IsNullOrEmpty(jobPageList.NextPageLink)));
        }

        /// <summary>
        /// This method validates the customer shipping address and provide alternate addresses
        /// if any.
        /// </summary>
        private static void ValidateShippingAddress()
        {
            AddressType addressType = AddressType.None;
            string companyName = "<company-name>";
            string streetAddress1 = "<street-address-1>";
            string streetAddress2 = "<street-address-2>";
            string streetAddress3 = "<street-address-3>";
            string postalCode = "<postal-code>";
            string city = "<city>";
            string stateOrProvince = "<state-or-province>";
            CountryCode countryCode = CountryCode.US;

            ShippingAddress shippingAddress = new ShippingAddress()
            {
                AddressType = addressType,
                CompanyName = companyName,
                StreetAddress1 = streetAddress1,
                StreetAddress2 = streetAddress2,
                StreetAddress3 = streetAddress3,
                City = city,
                StateOrProvince = stateOrProvince,
                PostalCode = postalCode,
                Country = countryCode.ToString(),
            };

            // Set location of the resource
            string location = "<location>";

            // Initializes a new instance of the DataBoxManagementClient class
            DataBoxManagementClient dataBoxManagementClient = InitializeDataBoxClient();
            dataBoxManagementClient.Location = location;

            ValidateAddress validateAddress = new ValidateAddress(shippingAddress, DeviceType.Pod);
            AddressValidationOutput addressValidationOutput = ServiceOperationsExtensions.ValidateAddressMethod(dataBoxManagementClient.Service, validateAddress);
        }

        /// <summary>
        /// Creates a new job with the specified parameters.
        /// </summary>
        private static void CreateJob()
        {
            AddressType addressType = AddressType.None;
            string streetAddress1 = "<street-address-1>";
            string streetAddress2 = "<street-address-2>";
            string streetAddress3 = "<street-address-3>";
            string postalCode = "<postal-code>";
            string city = "<city>";
            string stateOrProvince = "<state-or-province>";
            CountryCode countryCode = CountryCode.US;

            ShippingAddress shippingAddress = new ShippingAddress()
            {
                StreetAddress1 = streetAddress1,
                StreetAddress2 = streetAddress2,
                StreetAddress3 = streetAddress3,
                AddressType = addressType,
                Country = countryCode.ToString(),
                PostalCode = postalCode,
                City = city,
                StateOrProvince = stateOrProvince,
            };

            string emailIds = "<email-ids>";        // Input a semicolon (;) separated string of email ids, eg. "abc@outlook.com;xyz@outlook.com"
            string phoneNumber = "<phone-number>";
            string contactName = "<contact-name>";

            List<string> emailIdList = new List<string>();
            emailIdList = emailIds.Split(new char[';'], StringSplitOptions.RemoveEmptyEntries).ToList();

            ContactDetails contactDetails = new ContactDetails()
            {
                Phone = phoneNumber,
                EmailList = emailIdList,
                ContactName = contactName
            };

            string storageAccProviderType = "Microsoft.Storage"; // Microsoft.Storage / Microsoft.ClassicStorage
            string storageAccResourceGroupName = "<storage-account-resource-group-name>";
            string storageAccName = "<storage-account-name>";
            AccountType accountType = AccountType.GeneralPurposeStorage;

            List<DestinationAccountDetails> destinationAccountDetails = new List<DestinationAccountDetails>();
            destinationAccountDetails.Add(new DestinationAccountDetails(string.Concat("/subscriptions/", subscriptionId, "/resourceGroups/", storageAccResourceGroupName, "/providers/", storageAccProviderType, "/storageAccounts/", storageAccName.ToLower()), accountType));

            PodJobDetails jobDetails = new PodJobDetails(contactDetails, shippingAddress);

            string resourceGroupName = "<resource-group-name>";
            string location = "<location>";
            string jobName = "<job-or-order-name>";

            JobResource newJobResource = new JobResource(location, destinationAccountDetails, jobDetails);
            newJobResource.DeviceType = DeviceType.Pod;

            // Initializes a new instance of the DataBoxManagementClient class.
            DataBoxManagementClient dataBoxManagementClient = InitializeDataBoxClient();
            dataBoxManagementClient.Location = location;

            // Validate shipping address
            AddressValidationOutput addressValidateResult = ServiceOperationsExtensions.ValidateAddressMethod(dataBoxManagementClient.Service, new ValidateAddress(shippingAddress, newJobResource.DeviceType));

            if (addressValidateResult.ValidationStatus != AddressValidationStatus.Valid)
            {
                Console.WriteLine("Address validation status: {0}", addressValidateResult.ValidationStatus);

                if (addressValidateResult.ValidationStatus == AddressValidationStatus.Ambiguous)
                {
                    Console.WriteLine("\nSUPPORT ADDRESSES:");
                    foreach (ShippingAddress address in addressValidateResult.AlternateAddresses)
                    {
                        Console.WriteLine("Address type: {0}", address.AddressType);
                        if (!(string.IsNullOrEmpty(address.CompanyName))) Console.WriteLine("Company name: {0}", address.CompanyName);
                        if (!(string.IsNullOrEmpty(address.StreetAddress1))) Console.WriteLine("Street address1: {0}", address.StreetAddress1);
                        if (!(string.IsNullOrEmpty(address.StreetAddress2))) Console.WriteLine("Street address2: {0}", address.StreetAddress2);
                        if (!(string.IsNullOrEmpty(address.StreetAddress3))) Console.WriteLine("Street address3: {0}", address.StreetAddress3);
                        if (!(string.IsNullOrEmpty(address.City))) Console.WriteLine("City: {0}", address.City);
                        if (!(string.IsNullOrEmpty(address.StateOrProvince))) Console.WriteLine("State/Province: {0}", address.StateOrProvince);
                        if (!(string.IsNullOrEmpty(address.Country))) Console.WriteLine("Country: {0}", address.Country);
                        if (!(string.IsNullOrEmpty(address.PostalCode))) Console.WriteLine("Postal code: {0}", address.PostalCode);
                        if (!(string.IsNullOrEmpty(address.ZipExtendedCode))) Console.WriteLine("Zip extended code: {0}", address.ZipExtendedCode);
                        Console.WriteLine();
                    }
                }
                Console.ReadLine();
                return;
            }

            // Creates a new job.
            JobResource jobResource = JobsOperationsExtensions.Create(dataBoxManagementClient.Jobs, resourceGroupName, jobName, newJobResource);
        }

        /// <summary>
        /// This method cancels the specified job.
        /// </summary>
        private static void CancelJob()
        {
            string resourceGroupName = "<resource-group-name>";
            string jobName = "<job-name>";
            string reason = "<reason>";

            // Initializes a new instance of the DataBoxManagementClient class.
            DataBoxManagementClient dataBoxManagementClient = InitializeDataBoxClient();

            // Gets information about the specified job.
            JobResource jobResource = JobsOperationsExtensions.Get(
                                        dataBoxManagementClient.Jobs,
                                        resourceGroupName,
                                        jobName);

            if (jobResource.IsCancellable != null
                && (bool)jobResource.IsCancellable)
            {
                CancellationReason cancellationReason = new CancellationReason(reason);

                // Initiate cancel job
                JobsOperationsExtensions.Cancel(
                    dataBoxManagementClient.Jobs,
                    resourceGroupName,
                    jobName,
                    cancellationReason);
            }
        }

        /// <summary>
        /// This method deletes the specified job.
        /// </summary>
        private static void DeleteJob()
        {
            string resourceGroupName = "<resource-group-name>";
            string jobName = "<job-name>";

            // Initializes a new instance of the DataBoxManagementClient class.
            DataBoxManagementClient dataBoxManagementClient = InitializeDataBoxClient();

            // Gets information about the specified job.
            JobResource jobResource = JobsOperationsExtensions.Get(
                                        dataBoxManagementClient.Jobs,
                                        resourceGroupName,
                                        jobName);

            if (jobResource.Status == StageName.Cancelled
                || jobResource.Status == StageName.Completed
                || jobResource.Status == StageName.CompletedWithErrors)
            {
                // Initiate delete job
                JobsOperationsExtensions.Delete(dataBoxManagementClient.Jobs,
                    resourceGroupName,
                    jobName);
            }
        }

        /// <summary>
        /// This method gets shipping label sas uri for the specified job.
        /// </summary>
        private static void GetShippingLableUri()
        {
            string resourceGroupName = "<resource-group-name>";
            string jobName = "<job-name>";

            // Initializes a new instance of the DataBoxManagementClient class.
            DataBoxManagementClient dataBoxManagementClient = InitializeDataBoxClient();

            // Gets information about the specified job.
            JobResource jobResource = JobsOperationsExtensions.Get(
                                        dataBoxManagementClient.Jobs,
                                        resourceGroupName,
                                        jobName);

            if (jobResource.Status == StageName.Delivered)
            {
                // Initiate cancel job
                ShippingLabelDetails shippingLabelDetails = JobsOperationsExtensions.DownloadShippingLabelUri(
                                                                dataBoxManagementClient.Jobs, 
                                                                resourceGroupName,
                                                                jobName);

                Console.WriteLine("Shipping address sas url: \n{0}", shippingLabelDetails.ShippingLabelSasUri);
            }
        }

        /// <summary>
        /// Initializes a new instance of the ShipmentPickUpRequest class.
        /// </summary>
        private static void BookShipmentPickup()
        {
            string resourceGroupName = "<resoruce-group-name>";
            string jobName = "<job-name>";

            DateTime dtStartTime = new DateTime();
            DateTime dtEndTime = new DateTime();
            string shipmentLocation = "<shipment-location>";

            ShipmentPickUpRequest shipmentPickUpRequest = new ShipmentPickUpRequest(dtStartTime, dtEndTime, shipmentLocation);

            // Initializes a new instance of the DataBoxManagementClient class
            DataBoxManagementClient dataBoxManagementClient = InitializeDataBoxClient();

            // Gets information about the specified job.
            JobResource jobResource = JobsOperationsExtensions.Get(
                                        dataBoxManagementClient.Jobs,
                                        resourceGroupName,
                                        jobName);

            if (jobResource.Status == StageName.Delivered)
            {
                // Initiate Book shipment pick up
                ShipmentPickUpResponse shipmentPickUpResponse = JobsOperationsExtensions.BookShipmentPickUp(
                    dataBoxManagementClient.Jobs,
                    resourceGroupName,
                    jobName,
                    shipmentPickUpRequest);

                Console.WriteLine("Confirmation number: {0}", shipmentPickUpResponse.ConfirmationNumber);
            }
        }

        /// <summary>
        /// Provides list of copy logs uri.
        /// </summary>
        private static void GetCopyLogsUri()
        {
            string resourceGroupName = "<resource-group-name>";
            string jobName = "<job-name>";

            // Initializes a new instance of the DataBoxManagementClient class
            DataBoxManagementClient dataBoxManagementClient = InitializeDataBoxClient();

            // Gets information about the specified job.
            JobResource jobResource = JobsOperationsExtensions.Get(
                                        dataBoxManagementClient.Jobs,
                                        resourceGroupName,
                                        jobName);

            if (jobResource.Status == StageName.DataCopy
                || jobResource.Status == StageName.Completed
                || jobResource.Status == StageName.CompletedWithErrors)
            {
                // Fetches the Copy log details
                GetCopyLogsUriOutput getCopyLogsUriOutput =
                    JobsOperationsExtensions.GetCopyLogsUri(
                        dataBoxManagementClient.Jobs,
                        resourceGroupName,
                        jobName);

                if (getCopyLogsUriOutput.CopyLogDetails != null)
                {
                    Console.WriteLine("Copy log details");
                    foreach (AccountCopyLogDetails copyLogitem in getCopyLogsUriOutput.CopyLogDetails)
                    {
                        Console.WriteLine(string.Concat("  Account name: ", copyLogitem.AccountName, Environment.NewLine,
                            "  Copy log link: ", copyLogitem.CopyLogLink, Environment.NewLine, Environment.NewLine));
                    }
                }
            }
        }

        /// <summary>
        /// This method gets the unencrypted secrets related to the job.
        /// </summary>
        private static void GetSecrets()
        {
            string resourceGroupName = "<resource-group-name>";
            string jobName = "<job-name>";

            // Initializes a new instance of the DataBoxManagementClient class
            DataBoxManagementClient dataBoxManagementClient = InitializeDataBoxClient();

            // Gets information about the specified job.
            JobResource jobResource = JobsOperationsExtensions.Get(
                                        dataBoxManagementClient.Jobs,
                                        resourceGroupName,
                                        jobName);

            if (jobResource.Status != null
                && (int)jobResource.Status >= (int)StageName.Delivered
                && (int)jobResource.Status <= (int)StageName.DataCopy)
            {
                // Fetches the list of unencrypted secrets
                UnencryptedSecrets secrets = ListSecretsOperationsExtensions.ListByJobs(
                                                dataBoxManagementClient.ListSecrets,
                                                resourceGroupName,
                                                jobName);

                PodJobSecrets podSecret = (PodJobSecrets)secrets.JobSecrets;

                if (podSecret.PodSecrets != null)
                {
                    Console.WriteLine("Azure Databox device credentails");
                    foreach (PodSecret accountCredentials in podSecret.PodSecrets)
                    {
                        Console.WriteLine(" Device serial number: {0}", accountCredentials.DeviceSerialNumber);
                        Console.WriteLine(" Device password: {0}", accountCredentials.DevicePassword);

                        foreach (AccountCredentialDetails accountCredentialDetails in
                            accountCredentials.AccountCredentialDetails)
                        {
                            Console.WriteLine("  Account name: {0}", accountCredentialDetails.AccountName);
                            foreach (ShareCredentialDetails shareCredentialDetails in
                                    accountCredentialDetails.ShareCredentialDetails)
                            {
                                Console.WriteLine("   Share name: {0}", shareCredentialDetails.ShareName);
                                Console.WriteLine("   User name: {0}", shareCredentialDetails.UserName);
                                Console.WriteLine("   Password: {0}{1}", shareCredentialDetails.Password, Environment.NewLine);
                            }
                        }
                        Console.WriteLine();
                    }
                    Console.ReadLine();
                }
            }
        }
    }
}
