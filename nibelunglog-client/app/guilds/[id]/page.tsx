"use client";

import { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import { guildsApi } from "@/utils/api/guildsApi";
import { ApiErrorHandler } from "@/utils/api/errorHandler";
import { GuildHeader } from "@/components/guilds/detail/GuildHeader";
import { GuildMembersTable } from "@/components/guilds/detail/GuildMembersTable";
import { GuildMemberFiltersModal } from "@/components/guilds/detail/GuildMemberFiltersModal";
import { GuildStatisticsChart } from "@/components/guilds/detail/GuildStatisticsChart";
import { GuildTopPlayersChart } from "@/components/guilds/detail/GuildTopPlayersChart";
import { ErrorMessage } from "@/components/ui/error-message";
import { ArrowLeft, Filter, Search } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useGuildMemberFiltersStore } from "@/stores/guildMemberFiltersStore";
import type { GuildDetailDto } from "@/types/api/Guild";

export default function GuildDetailPage() {
  const params = useParams();
  const router = useRouter();
  const guildId = parseInt(params.id as string, 10);
  const { filters, updateFilter } = useGuildMemberFiltersStore();

  const [guild, setGuild] = useState<GuildDetailDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isFiltersOpen, setIsFiltersOpen] = useState(false);
  const [localSearch, setLocalSearch] = useState(filters.search || "");

  useEffect(() => {
    setLocalSearch(filters.search || "");
  }, [filters.search]);

  useEffect(() => {
    if (isNaN(guildId) || guildId <= 0) {
      setError("Неверный ID гильдии");
      setIsLoading(false);
      return;
    }

    const fetchGuild = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const result = await guildsApi.getGuildById(guildId);
        if (!result) {
          setError("Гильдия не найдена");
          setGuild(null);
        } else {
          setGuild(result);
          setError(null);
        }
      } catch (err) {
        const errorMessage = ApiErrorHandler.getErrorMessage(err);
        setError(errorMessage);
        setGuild(null);
      } finally {
        setIsLoading(false);
      }
    };

    fetchGuild();
  }, [guildId]);

  const handleSearchSubmit = () => {
    updateFilter("search", localSearch.trim() || undefined);
    updateFilter("page", 1);
  };

  if (isLoading)
    return (
      <div className="container mx-auto py-12 px-8 max-w-7xl">
        <div className="flex items-center justify-center min-h-[60vh]">
          <div className="text-muted-foreground text-lg">Загрузка...</div>
        </div>
      </div>
    );

  if (error && !guild)
    return (
      <div className="container mx-auto py-12 px-8 max-w-7xl">
        <Button
          variant="ghost"
          onClick={() => router.push("/guilds")}
          className="mb-6"
        >
          <ArrowLeft className="mr-2 size-4" />
          Назад к списку гильдий
        </Button>
        <ErrorMessage message={error} onRetry={() => window.location.reload()} />
      </div>
    );

  if (!guild)
    return (
      <div className="container mx-auto py-12 px-8 max-w-7xl">
        <Button
          variant="ghost"
          onClick={() => router.push("/guilds")}
          className="mb-6"
        >
          <ArrowLeft className="mr-2 size-4" />
          Назад к списку гильдий
        </Button>
        <ErrorMessage message="Гильдия не найдена" onRetry={() => window.location.reload()} />
      </div>
    );

  return (
    <div className="container mx-auto py-12 px-8 max-w-7xl">
      <Button
        variant="ghost"
        onClick={() => router.push("/guilds")}
        className="mb-6"
      >
        <ArrowLeft className="mr-2 size-4" />
        Назад к списку гильдий
      </Button>

      <GuildHeader guild={guild} />

      <div className="mt-8">
        <GuildStatisticsChart guildId={guildId} />
      </div>

      <div className="mt-8">
        <GuildTopPlayersChart guildId={guildId} />
      </div>

      <div className="mt-8">
        <div className="mb-4 flex gap-3">
          <div className="flex-1 relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Поиск по имени игрока..."
              value={localSearch}
              onChange={(e) => setLocalSearch(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter")
                  handleSearchSubmit();
              }}
              className="pl-10"
            />
          </div>
          <Button
            variant="outline"
            onClick={() => setIsFiltersOpen(true)}
            className="gap-2 h-11"
          >
            <Filter className="h-4 w-4" />
            Фильтр
          </Button>
        </div>
        <GuildMemberFiltersModal
          open={isFiltersOpen}
          onOpenChange={setIsFiltersOpen}
        />
        <GuildMembersTable guildId={guildId} />
      </div>
    </div>
  );
}
