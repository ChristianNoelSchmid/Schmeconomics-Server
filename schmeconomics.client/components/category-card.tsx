import { CategoryModel } from "@/api";

export function CategoryCard(
    props: {
        category: CategoryModel,
        onClick: (categoryId: string) => void
    }
) {
    return (
        <div 
            onClick={() => props.onClick(props.category.id)}
        >
            {props.category.name}
        </div>
    )
}