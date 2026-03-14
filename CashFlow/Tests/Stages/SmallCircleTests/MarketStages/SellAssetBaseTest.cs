using CashFlow.Data.Consts;
using CashFlow.Data.DTOs;
using CashFlow.Interfaces;
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
            AssetType.SmallBusinessType
        };

        PersonManagerMock.Setup(a => a.GetAssetDescription(It.IsAny<AssetDto>(), CurrentUserMock.Object))
            .Returns((AssetDto asset, ICashFlowUser user) => $"{asset.Title} Text");

        int id = 0;
        foreach (var type in assetTypes)
        {
            List<AssetDto> assetsOfType = [
                new AssetDto { Id = id++, Type = type, Qtty = 1, Title = $"{type} No1", CashFlow = 100 * id, MarkedToSell = false },
                new AssetDto { Id = id++, Type = type, Qtty = 1, Title = $"{type} No2", CashFlow = 100 * id, MarkedToSell = true },
                new AssetDto { Id = id++, Type = type, Qtty = 1, Title = $"{type} No3", CashFlow = 100 * id, MarkedToSell = false },
            ];

            PersonManagerMock.Setup(a => a.ReadAllAssets(type, CurrentUserMock.Object)).Returns(assetsOfType);
            Assets.AddRange(assetsOfType);
        }
    }
}
