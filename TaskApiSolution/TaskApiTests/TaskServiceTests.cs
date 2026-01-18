using NSubstitute;
using TaskApi.BusinessLogic.AuthApiService;
using TaskApi.BusinessLogic.MessagesCreator;
using TaskApi.BusinessLogic.Services;
using TaskApi.Common.Contracts.Request;
using TaskApi.Common.Contracts.Response;
using TaskApi.Common.Enums;
using TaskApi.Common.Events;
using TaskApi.Common.Events.Base;
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

        private readonly CancellationToken _cancellationToken= CancellationToken.None;

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

            _taskRepository.GetTaskByIdAsync(TASK_ID, _cancellationToken).Returns(_existingTask);
        }


        [Fact]
        public async Task CreateTaskAsync_Success()
        {
            //Arrange
            var createTaskRequest = new CreateTaskRequest
            {
                CreatorId = Guid.Empty
            };

            _taskRepository.GetTaskDtoByIdAsync(Arg.Any<int>(), _cancellationToken).Returns(new ReadTaskDto { Id = 1 });   

            //Act
            var result = await _sut.CreateTaskAsync(createTaskRequest, _cancellationToken);

            //Assert
            await _taskRepository.Received(1).SaveTaskAsync(Arg.Is<CreateTaskRequest>(x => x.CreatorId == _currentlyLoggedInUser.Id), _cancellationToken);
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task DeleteTask_Success()
        {
            //Arrange
            var taskRemovedMessage = new TaskRemovedEvent(default, default);
            _messageFactory.GetTaskRemovedEvent(_existingTask).Returns(taskRemovedMessage);

            //Act
            await _sut.DeleteTaskAsync(TASK_ID, _cancellationToken);

            //Assert
            await _taskRepository.Received(1).GetTaskByIdAsync(TASK_ID, _cancellationToken);
            await _taskRepository.Received(1).DeleteTask(_existingTask, _cancellationToken);
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
            await _sut.UpdateTaskAsync(updateTaskDto, _cancellationToken);

            //Assert
            await _taskRepository.Received(1).UpdateTaskAsync(Arg.Is<Tasks>(x => x.Title == newTitle), _cancellationToken);
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
            await _sut.UpdateTaskAsync(updateTaskDto, _cancellationToken);

            //Assert
            await _taskRepository.DidNotReceiveWithAnyArgs().UpdateTaskAsync(Arg.Any<Tasks>(), Arg.Any<CancellationToken>());
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
            await _sut.UpdateTaskStatusAsync(updateTaskStatusDto, _cancellationToken);

            //Assert
            await _taskRepository.Received(1).UpdateTaskStatusAsync(_existingTask, newStatus, _cancellationToken);
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
            await _sut.UpdateTaskStatusAsync(updateTaskStatusDto, _cancellationToken);

            //Assert
            await _taskRepository.DidNotReceive().UpdateTaskStatusAsync(Arg.Any<Tasks>(), Arg.Any<TaskStatuses>(), Arg.Any<CancellationToken>());
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
            await _sut.ChangeTaskAssignmentAsync(updateTaskAssigmentDto, _cancellationToken);

            //Assert
            await _taskRepository.Received(1).UpdateAssignmentAsync(_existingTask, updateTaskAssigmentDto, _cancellationToken);
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
            await _sut.ChangeTaskAssignmentAsync(updateTaskAssigmentDto, _cancellationToken);

            //Assert
            await _taskRepository.DidNotReceive().UpdateAssignmentAsync(Arg.Any<Tasks>(), Arg.Any<UpdateTaskAssigmentDto>(), Arg.Any<CancellationToken>());
            _messageFactory.DidNotReceive().GetTaskAssignementUpdatedEvent(Arg.Any<Tasks>(), Arg.Any<UserInfo>());
            await _messageClient.DidNotReceive().SendMessage(Arg.Any<ITaskEvent>());
        }
    }
}
