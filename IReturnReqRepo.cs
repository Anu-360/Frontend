using Hexa_Hub.DTO;

namespace Hexa_Hub.Interface
{
    public interface IReturnReqRepo
    {
        Task<List<ReturnRequest>> GetAllReturnRequest();
        Task<ReturnRequest?> GetReturnRequestById(int id);
        Task<ReturnRequest> AddReturnRequest(ReturnRequestDto returnRequestDto);
        public void UpdateReturnRequest(ReturnRequest returnRequest);
        Task DeleteReturnRequest(int id);
        Task Save();
        Task<List<ReturnRequest>> GetReturnRequestsByUserId(int userId);

        Task<bool> UserHasAsset(int id);
    }
}
