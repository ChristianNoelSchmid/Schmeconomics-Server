import { AccountCard } from "@/components/account-card";
import { AccountModel } from "../../api";

export default function Home(accounts: [AccountModel]) {
    {accounts.map((account, i) => <AccountCard account={account} onClick={() => {}} />)}
}