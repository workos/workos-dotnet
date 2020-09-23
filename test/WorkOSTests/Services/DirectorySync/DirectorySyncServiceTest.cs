namespace WorkOSTests
{
    using System;
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
                ProjectId = "project_123",
            };

            this.mockUser = new User
            {
                Id = "directory_user_123",
                FirstName = "Rick",
                LastName = "Sanchez",
                Username = "rick.sanchez",
                Emails = new User.Email[]
                {
                    new User.Email
                    {
                        Primary = true,
                        Value = "rick.sanchez@foo-corp.com",
                        Type = "work",
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
        public void TestListDirectories()
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

            var response = this.service.ListDirectories();

            this.httpMock.AssertRequestWasMade(HttpMethod.Get, "/directories");
            Assert.Equal(
                JsonConvert.SerializeObject(mockResponse),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestListDirectoriesAsync()
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

            var response = await this.service.ListDirectoriesAsync();

            this.httpMock.AssertRequestWasMade(HttpMethod.Get, "/directories");
            Assert.Equal(
                JsonConvert.SerializeObject(mockResponse),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public void TestListUsers()
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

            var response = this.service.ListUsers(
                new ListUsersOptions
                {
                    Directory = "directory_123",
                });

            this.httpMock.AssertRequestWasMade(HttpMethod.Get, "/directory_users");
            Assert.Equal(
                JsonConvert.SerializeObject(mockResponse),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestListUsersAsync()
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

            var response = await this.service.ListUsersAsync(
                new ListUsersOptions
                {
                    Directory = "directory_123",
                });

            this.httpMock.AssertRequestWasMade(HttpMethod.Get, "/directory_users");
            Assert.Equal(
                JsonConvert.SerializeObject(mockResponse),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public void TestGetUser()
        {
            this.httpMock.MockResponse(
                HttpMethod.Get,
                $"/directory_users/{this.mockUser.Id}",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockUser));

            var response = this.service.GetUser(this.mockUser.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Get,
                $"/directory_users/{this.mockUser.Id}");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockUser),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestGetUserAsync()
        {
            this.httpMock.MockResponse(
                HttpMethod.Get,
                $"/directory_users/{this.mockUser.Id}",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockUser));

            var response = await this.service.GetUserAsync(this.mockUser.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Get,
                $"/directory_users/{this.mockUser.Id}");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockUser),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public void TestListGroups()
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

            var response = this.service.ListGroups(
                new ListGroupsOptions
                {
                    Directory = "directory_123",
                });

            this.httpMock.AssertRequestWasMade(HttpMethod.Get, "/directory_groups");
            Assert.Equal(
                JsonConvert.SerializeObject(mockResponse),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestListGroupsAsync()
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

            var response = await this.service.ListGroupsAsync(
                new ListGroupsOptions
                {
                    Directory = "directory_123",
                });

            this.httpMock.AssertRequestWasMade(HttpMethod.Get, "/directory_groups");
            Assert.Equal(
                JsonConvert.SerializeObject(mockResponse),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public void TestGetGroup()
        {
            this.httpMock.MockResponse(
                HttpMethod.Get,
                $"/directory_groups/{this.mockGroup.Id}",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockGroup));

            var response = this.service.GetGroup(this.mockGroup.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Get,
                $"/directory_groups/{this.mockGroup.Id}");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockGroup),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestGetGroupAsync()
        {
            this.httpMock.MockResponse(
                HttpMethod.Get,
                $"/directory_groups/{this.mockGroup.Id}",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockGroup));

            var response = await this.service.GetGroupAsync(this.mockGroup.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Get,
                $"/directory_groups/{this.mockGroup.Id}");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockGroup),
                JsonConvert.SerializeObject(response));
        }
    }
}
