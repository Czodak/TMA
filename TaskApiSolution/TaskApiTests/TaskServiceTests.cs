using System.Threading.Tasks;
using NSubstitute;
using TaskApi.BusinessLogic.AuthApiService;
using TaskApi.BusinessLogic.MessagesCreator;
using TaskApi.BusinessLogic.Services;
using TaskApi.Common.Contracts.Request;
using TaskApi.Common.Enums;
using TaskApi.Common.Events;
using TaskApi.Common.Events.Base;
using TaskApi.Common.Exceptions;
using TaskApi.Common.HttpClients.Auth;
using TaskApi.Contracts.Request;
using TaskApi.Data.Entities;
using TaskApi.Data.Repositories;
using TaskApi.Messaging.Client;

namespace TaskApiTests
{
    public class TaskServiceTests
    {
        private ITaskRepository _taskRepository;
        private IMessageClient _messageClient;
        private IMessageFactory _messageFactory;
        private IAuthApiService _authApiService;

        private ITaskService _sut;

        private readonly UserInfo _currentlyLoggedInUser;
        private const int TASK_ID = 1;
        private readonly Tasks _existingTask;

        public TaskServiceTests()
        {
            _taskRepository = Substitute.For<ITaskRepository>();
            _messageClient = Substitute.For<IMessageClient>();
            _messageFactory = Substitute.For<IMessageFactory>();
            _authApiService = Substitute.For<IAuthApiService>();

            _sut = new TaskService(_taskRepository, _authApiService, _messageClient, _messageFactory);

            _currentlyLoggedInUser = new UserInfo
            {
                Email = "test@test.test",
                Id = Guid.NewGuid()
            };

            _authApiService.MeAsync().Returns(_currentlyLoggedInUser);
            _existingTask = new Tasks
            {
                Id = TASK_ID,
                Status = TaskStatuses.New,
                CurrentlyAssignedUserId = _currentlyLoggedInUser.Id
            };

            _taskRepository.GetTaskByIdAsync(TASK_ID).Returns(_existingTask);
        }


        [Fact]
        public async Task CreateTaskAsync_Success()
        {
            //Arrange
            var createTaskRequest = new CreateTaskRequest
            {
                CreatorId = Guid.Empty
            };


            //Act
            await _sut.CreateTaskAsync(createTaskRequest);

            //Assert
            await _taskRepository.Received(1).SaveTaskAsync(Arg.Is<CreateTaskRequest>(x => x.CreatorId == _currentlyLoggedInUser.Id));
        }

        [Fact]
        public async Task CreateTaskAsync_NullUser_ThrowsException()
        {
            //Arrange
            var createTaskRequest = new CreateTaskRequest();
            UserInfo? nullUser = null;
            _authApiService.MeAsync().Returns(nullUser);

            //Act && Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.CreateTaskAsync(createTaskRequest));
        }

        [Fact]
        public async Task DeleteTask_Success()
        {
            //Arrange
            var taskRemovedMessage = new TaskRemovedEvent(default, default);
            _messageFactory.GetTaskRemovedEvent(_existingTask).Returns(taskRemovedMessage);

            //Act
            await _sut.DeleteTask(TASK_ID);

            //Assert
            await _taskRepository.Received(1).GetTaskByIdAsync(TASK_ID);
            await _taskRepository.Received(1).DeleteTask(_existingTask);
            await _messageFactory.Received(1).GetTaskRemovedEvent(_existingTask);
            await _messageClient.Received(1).SendMessage(taskRemovedMessage);
        }

        [Fact]
        public async Task UpdateTaskAsync_Success()
        {
            //Arrange
            var newTitle = "new title";
            var updateTaskDto = new UpdateTaskDto
            {
                Id = TASK_ID,
                Title = newTitle
            };

            //Act
            await _sut.UpdateTaskAsync(updateTaskDto);

            //Assert
            await _taskRepository.Received(1).UpdateTaskAsync(Arg.Is<Tasks>(x => x.Title == newTitle));
        }

        [Fact]
        public async Task UpdateTaskAsync_NonePropertyUpdated_DoesNotCallUpdateMethod()
        {
            //Arrange
            var updateTaskDto = new UpdateTaskDto
            {
                Id = TASK_ID,
            };

            //Act
            await _sut.UpdateTaskAsync(updateTaskDto);

            //Assert
            await _taskRepository.DidNotReceiveWithAnyArgs().UpdateTaskAsync(Arg.Any<Tasks>());
        }

        [Fact]
        public async Task UpdateTaskStatusAsync_Success()
        {
            //Arrange
            var newStatus = TaskStatuses.Done;
            var updateTaskStatusDto = new UpdateTaskStatusDto
            {
                Id = TASK_ID,
                Status = newStatus
            };

            var taskUpdatedEvent = new TaskStatusUpdatedEvent(string.Empty, string.Empty);
            _messageFactory.GetTaskStatusUpdatedEvent(_existingTask).Returns(taskUpdatedEvent);

            //Act
            await _sut.UpdateTaskStatusAsync(updateTaskStatusDto);

            //Assert
            await _taskRepository.Received(1).UpdateTaskStatusAsync(_existingTask, newStatus);
            await _messageFactory.Received(1).GetTaskStatusUpdatedEvent(_existingTask);
            await _messageClient.Received(1).SendMessage(taskUpdatedEvent);
        }

        [Fact]
        public async Task UpdateTaskStatusAsync_SameStatus_DoesNotUpdateNorSendMessage()
        {
            //Arrange
            var updateTaskStatusDto = new UpdateTaskStatusDto
            {
                Id = TASK_ID,
                Status = _existingTask.Status
            };

            //Act
            await _sut.UpdateTaskStatusAsync(updateTaskStatusDto);

            //Assert
            await _taskRepository.DidNotReceive().UpdateTaskStatusAsync(Arg.Any<Tasks>(), Arg.Any<TaskStatuses>());
            await _messageFactory.DidNotReceive().GetTaskStatusUpdatedEvent(Arg.Any<Tasks>());
            await _messageClient.DidNotReceive().SendMessage(Arg.Any<TaskStatusUpdatedEvent>());
        }

        [Fact]
        public async Task ChangeTaskAssignment_Success()
        {
            //Arrange
            var updateTaskAssigmentDto = new UpdateTaskAssigmentDto
            {
                Id = TASK_ID,
                NewAssignedUser = Guid.NewGuid()
            };
            var user = new UserInfo();
            _authApiService.GetUserById(updateTaskAssigmentDto.NewAssignedUser).Returns(user);

            var message = new TaskAssignementUpdatedEvent(string.Empty, string.Empty);
            _messageFactory.GetTaskAssignementUpdatedEvent(_existingTask, user).Returns(message);

            //Act
            await _sut.ChangeTaskAssignment(updateTaskAssigmentDto);

            //Assert
            await _taskRepository.Received(1).UpdateAssignmentAsync(_existingTask, updateTaskAssigmentDto);
            _messageFactory.Received(1).GetTaskAssignementUpdatedEvent(_existingTask, user);
            await _messageClient.Received(1).SendMessage(message);
        }
        
        
        [Fact]
        public async Task ChangeTaskAssignment_SameUser_DoNothing()
        {
            //Arrange
            var updateTaskAssigmentDto = new UpdateTaskAssigmentDto
            {
                Id = TASK_ID,
                NewAssignedUser = _currentlyLoggedInUser.Id
            };
            _authApiService.GetUserById(updateTaskAssigmentDto.NewAssignedUser).Returns(_currentlyLoggedInUser);

            //Act
            await _sut.ChangeTaskAssignment(updateTaskAssigmentDto);

            //Assert
            await _taskRepository.DidNotReceive().UpdateAssignmentAsync(Arg.Any<Tasks>(), Arg.Any<UpdateTaskAssigmentDto>());
            _messageFactory.DidNotReceive().GetTaskAssignementUpdatedEvent(Arg.Any<Tasks>(), Arg.Any<UserInfo>());
            await _messageClient.DidNotReceive().SendMessage(Arg.Any<ITaskEvent>());
        }

        [Fact]
        public async Task ChangeTaskAssignment_NewUserNotFound_ThrowsException()
        {
            //Arrange
            var updateTaskAssigmentDto = new UpdateTaskAssigmentDto
            {
                Id = TASK_ID,
                NewAssignedUser = Guid.NewGuid()
            };
            UserInfo? user = null;
            _authApiService.GetUserById(updateTaskAssigmentDto.NewAssignedUser).Returns(user);

            //Act
            await Assert.ThrowsAsync<NotFoundException>(() => _sut.ChangeTaskAssignment(updateTaskAssigmentDto));
            
            //Assert
            await _taskRepository.DidNotReceive().UpdateAssignmentAsync(Arg.Any<Tasks>(), Arg.Any<UpdateTaskAssigmentDto>());
            _messageFactory.DidNotReceive().GetTaskAssignementUpdatedEvent(Arg.Any<Tasks>(), Arg.Any<UserInfo>());
            await _messageClient.DidNotReceive().SendMessage(Arg.Any<ITaskEvent>());
        }
    }
}
