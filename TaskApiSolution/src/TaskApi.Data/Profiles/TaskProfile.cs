using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using TaskApi.Common.Contracts.Response;
using TaskApi.Contracts.Request;
using TaskApi.Data.Entities;

namespace TaskApi.Common.Profiles
{
    public class TaskProfile : Profile
    {
        public TaskProfile() 
        {
            //source -> target
            CreateMap<Tasks, ReadTaskDto>();
            CreateMap<CreateTaskRequest, Tasks>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedAt, opt => opt.Ignore());

        }
    }
}
