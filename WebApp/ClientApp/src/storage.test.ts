import { beforeEach, describe, expect, it } from 'vitest'
import { emptyShoppingList, loadShoppingList, saveShoppingList } from './storage'

describe('shopping-list storage', () => {
  beforeEach(() => localStorage.clear())

  it('starts with an empty list', () => {
    expect(loadShoppingList()).toEqual(emptyShoppingList)
  })

  it('round-trips a saved list', () => {
    const list = {
      recipeIds: ['recipe-id'],
      recipeTitles: { 'recipe-id': 'Brownies' },
      items: ['12 eggs'],
      checkedKeys: ['egg-id'],
    }
    saveShoppingList(list)
    expect(loadShoppingList()).toEqual(list)
  })

  it('recovers from invalid storage', () => {
    localStorage.setItem('kitchenpc-sample-shopping-list', '{bad json')
    expect(loadShoppingList()).toEqual(emptyShoppingList)
  })
})
