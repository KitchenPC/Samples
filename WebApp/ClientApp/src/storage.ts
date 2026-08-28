import type { SavedShoppingList } from './types'

const storageKey = 'kitchenpc-sample-shopping-list'
export const emptyShoppingList: SavedShoppingList = {
  recipeIds: [],
  recipeTitles: {},
  items: [],
  checkedKeys: [],
}

export function loadShoppingList(): SavedShoppingList {
  try {
    const value = localStorage.getItem(storageKey)
    if (!value) return emptyShoppingList
    const parsed = JSON.parse(value) as Partial<SavedShoppingList>
    return {
      recipeIds: Array.isArray(parsed.recipeIds) ? parsed.recipeIds : [],
      recipeTitles: parsed.recipeTitles ?? {},
      items: Array.isArray(parsed.items) ? parsed.items : [],
      checkedKeys: Array.isArray(parsed.checkedKeys) ? parsed.checkedKeys : [],
    }
  } catch {
    return emptyShoppingList
  }
}

export function saveShoppingList(value: SavedShoppingList) {
  localStorage.setItem(storageKey, JSON.stringify(value))
}
