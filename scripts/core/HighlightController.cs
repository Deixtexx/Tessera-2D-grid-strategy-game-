using Godot;
using System.Collections.Generic;

public class HighlightController
{
    private TileMapLayer _moveLayer;
    private TileMapLayer _attackLayer;
    private TileMapLayer _previewLayer;
    private TileMapLayer _abilityRangeLayer;

    private int _moveSourceId, _attackSourceId, _previewSourceId, _abilityRangeSourceId;
    private Vector2I _moveAtlas, _attackAtlas, _previewAtlas, _abilityRangeAtlas;

    public HighlightController(
        TileMapLayer moveLayer, int moveSourceId, Vector2I moveAtlas,
        TileMapLayer attackLayer, int attackSourceId, Vector2I attackAtlas,
        TileMapLayer previewLayer, int previewSourceId, Vector2I previewAtlas,
        TileMapLayer abilityRangeLayer, int abilityRangeSourceId, Vector2I abilityRangeAtlas)
    {
        _moveLayer = moveLayer;
        _moveSourceId = moveSourceId;
        _moveAtlas = moveAtlas;

        _attackLayer = attackLayer;
        _attackSourceId = attackSourceId;
        _attackAtlas = attackAtlas;

        _previewLayer = previewLayer;
        _previewSourceId = previewSourceId;
        _previewAtlas = previewAtlas;

        _abilityRangeLayer = abilityRangeLayer;
        _abilityRangeSourceId = abilityRangeSourceId;
        _abilityRangeAtlas = abilityRangeAtlas;
    }

    public void ShowMoveCells(List<Vector2I> cells)
    {
        ClearAll();
        if (_moveLayer == null) return;
        foreach (var cell in cells)
            _moveLayer.SetCell(cell, _moveSourceId, _moveAtlas);
    }

    public void ShowAttackCells(List<Vector2I> cells)
    {
        ClearAll();
        if (_attackLayer == null) return;
        foreach (var cell in cells)
            _attackLayer.SetCell(cell, _attackSourceId, _attackAtlas);
    }

    public void ShowPreviewCells(List<Vector2I> cells)
    {
        if (_previewLayer == null) return;
        _previewLayer.Clear();
        foreach (var cell in cells)
            _previewLayer.SetCell(cell, _previewSourceId, _previewAtlas);
    }

    public void ClearPreview()
    {
        _previewLayer?.Clear();
    }

    public void ShowAbilityRange(List<Vector2I> cells)
    {
        _abilityRangeLayer?.Clear();
        if (_abilityRangeLayer == null) return;
        foreach (var cell in cells)
            _abilityRangeLayer.SetCell(cell, _abilityRangeSourceId, _abilityRangeAtlas);
    }

    public void ClearAbilityRange()
    {
        _abilityRangeLayer?.Clear();
    }

    public void ClearAll()
    {
        _moveLayer?.Clear();
        _attackLayer?.Clear();
        _previewLayer?.Clear();
        _abilityRangeLayer?.Clear();
    }
}
