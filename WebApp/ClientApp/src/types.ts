export interface RecipeSummary {
  id: string
  title: string
  description?: string
  imageUrl?: string
  author?: string
  prepTime?: number
  cookTime?: number
  averageRating: number
}

export interface RecipeList {
  recipes: RecipeSummary[]
  totalCount: number
}

export interface RecipeDetail extends RecipeSummary {
  credit?: string
  creditUrl?: string
  servingSize: number
  tags: string[]
  ingredients: { text: string }[]
  method?: string
}

export interface ShoppingListResult {
  items: { key: string; name: string; amount?: string }[]
  unrecognizedItems: string[]
}

export interface SavedShoppingList {
  recipeIds: string[]
  recipeTitles: Record<string, string>
  items: string[]
  checkedKeys: string[]
}
