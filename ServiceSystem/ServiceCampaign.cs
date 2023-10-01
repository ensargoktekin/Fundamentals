using System.Threading.Tasks;
using Game.Scripts.Model;

public class ServiceCampaign : BaseService {

    public async Task<OperationResult<BonusModel>> GetCampaignList(int type) {
        string url = Constants.GetCampaignUrl() + "bonus?gameId=" + Constants.gameId + "&typeIds=" + type.ToString();
        return await Get<BonusModel>(url);
    }

    public async Task<OperationPostResult> PostCampaignClaim(int bonusId) {
        string url = Constants.GetCampaignUrl() + "bonus?gameId=" + Constants.gameId + "&bonusId=" + bonusId.ToString();
        return await Post(url, null);
    }

    // More Get,Post&Put functions can be added to this class
}


