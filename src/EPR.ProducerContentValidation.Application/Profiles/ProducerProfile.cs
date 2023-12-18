using AutoMapper;
using EPR.ProducerContentValidation.Application.DTOs.SplitFunction;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;

namespace EPR.ProducerContentValidation.Application.Profiles;

public class ProducerProfile : Profile
{
    public ProducerProfile()
    {
        CreateMap<Producer, ProducerValidationInRequest>().ReverseMap();
        CreateMap<Producer, SubmissionEventRequest>()
            .ForMember(
                d => d.Errors,
                opt => opt.MapFrom(s => new List<string>()))
            .ForMember(
                d => d.ValidationErrors,
                opt => opt.MapFrom(s => new List<ProducerValidationEventIssueRequest>()))
            .ReverseMap();
        CreateMap<ProducerRow, ProducerRowInRequest>().ReverseMap();
        CreateMap<ProducerRow, ProducerValidationEventIssueRequest>().ReverseMap();
    }
}