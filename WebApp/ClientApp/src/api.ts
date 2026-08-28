import type { RecipeDetail, RecipeList, ShoppingListResult } from './types'

async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const response = await fetch(url, options)
  if (!response.ok) {
    throw new Error(response.status === 404 ? 'That recipe could not be found.' : 'KitchenPC could not complete the request.')
  }
  return response.json() as Promise<T>
}

export function searchRecipes(query: string, signal?: AbortSignal) {
  const params = new URLSearchParams()
  if (query.trim()) params.set('query', query.trim())
  const suffix = params.size ? `?${params}` : ''
  return request<RecipeList>(`/api/recipes${suffix}`, { signal })
}

export function getRecipe(id: string, signal?: AbortSignal) {
  return request<RecipeDetail>(`/api/recipes/${encodeURIComponent(id)}`, { signal })
}

export function aggregateShoppingList(recipeIds: string[], items: string[], signal?: AbortSignal) {
  return request<ShoppingListResult>('/api/shopping-list/aggregate', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ recipeIds, items }),
    signal,
  })
}
