import statistics

def calculate_median(values):
    if not values: return 0
    return statistics.median(values)

def filter_outliers_mad(prices, threshold=3.5):
    if len(prices) < 3: return prices
    
    prices.sort()
    median = calculate_median(prices)
    
    # Absolute deviations from median
    deviations = sorted([abs(p - median) for p in prices])
    mad = calculate_median(deviations)
    
    if abs(mad) < 0.0001: return prices
    
    filtered = []
    for p in prices:
        # Modified Z-score calculation
        z_score = 0.6745 * abs(p - median) / mad
        if z_score <= threshold:
            filtered.append(p)
    return filtered

def filter_outliers_iqr(prices):
    if len(prices) < 4: return prices
    
    prices.sort()
    n = len(prices)
    q1 = calculate_median(prices[:n//2])
    q3 = calculate_median(prices[(n+1)//2:])
    iqr = q3 - q1
    
    lower_bound = q1 - (1.5 * iqr)
    upper_bound = q3 + (1.5 * iqr)
    
    return [p for p in prices if p >= lower_bound and p <= upper_bound]

# --- SAMPLE DATA ---
# Imagine an item usually sold for ~10,000 gil.
# A bot puts one at 500 gil to trick other bots (Bait).
# Another person puts one at 50,000 gil (Overpriced).
market_prices = [500, 9800, 9900, 9950, 10000, 10100, 10200, 10500, 50000]

print(f"Original Market: {market_prices}")
print("-" * 30)

filtered_mad = filter_outliers_mad(market_prices)
print(f"MAD Filtered (Goal: remove 500 & 50000): {filtered_mad}")

filtered_iqr = filter_outliers_iqr(market_prices)
print(f"IQR Filtered: {filtered_iqr}")

print("-" * 30)
print(f"Resulting 'Match' Price: {min(filtered_mad)}")
