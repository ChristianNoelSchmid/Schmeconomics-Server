import { CategoryModel } from "@/api";
import { CategoryCard } from "@/components/category-card";

export default function Home(accounts: [CategoryModel]) {
    {accounts.map((account, i) => <CategoryCard category={account} onClick={() => {}} />)}
}