using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Data.Users;
using Moq;

namespace CashFlow.Tests.Stages.SmallCircleTests.MarketStages;

public abstract class SellAssetBaseTest : StagesBaseTest
{
    protected static List<AssetDto> Assets;

    [SetUp]
    public void Setup()
    {
        Assets = [];
        var assetTypes = new[]
        {
            AssetType.RealEstate,
            AssetType.Land,
            AssetType.Business,
            AssetType.Coin,
            AssetType.Stock,
            AssetType.SmallBusiness
        };

        AssetManagerMock.Setup(a => a.GetDescription(It.IsAny<AssetDto>(), CurrentUserMock.Object))
            .Returns((AssetDto asset, IUser user) => $"{asset.Title} Text");

        int id = 0;
        foreach (var type in assetTypes)
        {
            List<AssetDto> assetsOfType = [
                new AssetDto { Type = type, Id = id++, Qtty = 1, Title = $"{type} No1", CashFlow = 100 * id, MarkedToSell = true },
                new AssetDto { Type = type, Id = id++, Qtty = 1, Title = $"{type} No2", CashFlow = 100 * id, MarkedToSell = false },
                new AssetDto { Type = type, Id = id++, Qtty = 1, Title = $"{type} No3", CashFlow = 100 * id, MarkedToSell = true },
            ];

            PersonManagerMock.Setup(a => a.ReadAllAssets(type, CurrentUserMock.Object.Id)).Returns(assetsOfType);
            Assets.AddRange(assetsOfType);
        }
    }
}
