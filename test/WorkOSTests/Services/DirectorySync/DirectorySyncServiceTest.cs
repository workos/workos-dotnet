namespace WorkOSTests
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Newtonsoft.Json;
    using WorkOS;
    using Xunit;

    public class DirectorySyncServiceTest
    {
        private readonly HttpMock httpMock;

        private readonly DirectorySyncService service;

        private readonly ListDirectoriesOptions listDirectoriesOptions;

        private readonly ListUsersOptions listUsersOptions;

        private readonly ListGroupsOptions listGroupsOptions;

        private readonly Directory mockDirectory;

        private readonly User mockUser;

        private readonly Group mockGroup;

        public DirectorySyncServiceTest()
        {
            this.httpMock = new HttpMock();
            var client = new WorkOSClient(
                new WorkOSOptions
                {
                    ApiKey = "test",
                    HttpClient = this.httpMock.HttpClient,
                });
            this.service = new DirectorySyncService(client);

            this.listDirectoriesOptions = new ListDirectoriesOptions
            {
                Domain = "foo-corp.com",
            };

            this.listUsersOptions = new ListUsersOptions
            {
                Directory = "directory_123",
            };

            this.listGroupsOptions = new ListGroupsOptions
            {
                Directory = "directory_123",
            };

            this.mockDirectory = new Directory
            {
                Id = "directory_123",
                Name = "Foo Corp - BambooHR",
                Domain = "foo-corp.com",
                State = DirectoryState.Linked,
                Type = DirectoryType.BambooHR,
                ExternalKey = "external-key",
                OrganizationId = "organization_123",
                CreatedAt = "2021-07-26T18:55:16.072Z",
                UpdatedAt = "2021-07-26T18:55:16.072Z",
            };

            this.mockUser = new User
            {
                Id = "directory_user_123",
                FirstName = "Rick",
                LastName = "Sanchez",
                Username = "rick.sanchez",
                State = UserState.Active,
                CustomAttributes = new Dictionary<string, object>()
                {
                    { "manager_id", "123" },
                },
                Emails = new User.Email[]
                {
                    new User.Email
                    {
                        Primary = true,
                        Value = "rick.sanchez@foo-corp.com",
                        Type = "work",
                    },
                },
                Groups = new List<User.Group>
                {
                    new User.Group
                    {
                        Id = "directory_group_123",
                        Name = "Scientists",
                    },
                },
            };

            this.mockGroup = new Group
            {
                Id = "directory_group_123",
                Name = "Scientists",
            };
        }

        [Fact]
        public async void TestGetDirectory()
        {
            this.httpMock.MockResponse(
                HttpMethod.Get,
                $"/directories/{this.mockDirectory.Id}",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockDirectory));

            var response = await this.service.GetDirectory(this.mockDirectory.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Get,
                $"/directories/{this.mockDirectory.Id}");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockDirectory),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestListDirectories()
        {
            var mockResponse = new WorkOSList<Directory>
            {
                Data = new List<Directory>
                {
                    this.mockDirectory,
                },
            };
            this.httpMock.MockResponse(
                HttpMethod.Get,
                "/directories",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(mockResponse));

            var response = await this.service.ListDirectories(this.listDirectoriesOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Get, "/directories");
            Assert.Equal(
                JsonConvert.SerializeObject(mockResponse),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestDeleteDirectory()
        {
            this.httpMock.MockResponse(
                HttpMethod.Delete,
                $"/directories/{this.mockDirectory.Id}",
                HttpStatusCode.Accepted,
                "Accepted");

            await this.service.DeleteDirectory(this.mockDirectory.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Delete,
                $"/directories/{this.mockDirectory.Id}");
        }

        [Fact]
        public async void TestListUsers()
        {
            var mockResponse = new WorkOSList<User>
            {
                Data = new List<User>
                {
                    this.mockUser,
                },
            };
            this.httpMock.MockResponse(
                HttpMethod.Get,
                "/directory_users",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(mockResponse));

            var response = await this.service.ListUsers(this.listUsersOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Get, "/directory_users");
            Assert.Equal(
                JsonConvert.SerializeObject(mockResponse),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestGetUser()
        {
            this.httpMock.MockResponse(
                HttpMethod.Get,
                $"/directory_users/{this.mockUser.Id}",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockUser));

            var response = await this.service.GetUser(this.mockUser.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Get,
                $"/directory_users/{this.mockUser.Id}");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockUser),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestListGroups()
        {
            var mockResponse = new WorkOSList<Group>
            {
                Data = new List<Group>
                {
                    this.mockGroup,
                },
            };
            this.httpMock.MockResponse(
                HttpMethod.Get,
                "/directory_groups",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(mockResponse));

            var response = await this.service.ListGroups(this.listGroupsOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Get, "/directory_groups");
            Assert.Equal(
                JsonConvert.SerializeObject(mockResponse),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestGetGroup()
        {
            this.httpMock.MockResponse(
                HttpMethod.Get,
                $"/directory_groups/{this.mockGroup.Id}",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockGroup));

            var response = await this.service.GetGroup(this.mockGroup.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Get,
                $"/directory_groups/{this.mockGroup.Id}");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockGroup),
                JsonConvert.SerializeObject(response));
        }
    }
}
